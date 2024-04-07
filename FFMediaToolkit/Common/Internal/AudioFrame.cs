﻿namespace FFMediaToolkit.Common.Internal
{
    using System;
    using FFMediaToolkit.Audio;
    using FFMediaToolkit.Helpers;
    using FFmpeg.AutoGen;

    /// <summary>
    /// Represent an audio frame.
    /// </summary>
    internal unsafe class AudioFrame : MediaFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFrame"/> class with empty frame data.
        /// </summary>
        public AudioFrame()
            : base(ffmpeg.av_frame_alloc())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioFrame"/> class using existing <see cref="AVFrame"/>.
        /// </summary>
        /// <param name="frame">The audio <see cref="AVFrame"/>.</param>
        public AudioFrame(AVFrame* frame)
            : base(frame)
        {
            if (frame->GetMediaType() != MediaType.Audio)
                throw new ArgumentException("Cannot create an AudioFrame instance from the AVFrame with type: " + frame->GetMediaType());
        }

        /// <summary>
        /// Gets the number of samples.
        /// </summary>
        public int NumSamples => Pointer != null ? Pointer->nb_samples : default;

        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        public int SampleRate => Pointer != null ? Pointer->sample_rate : default;

        /// <summary>
        /// Gets the number of channels.
        /// </summary>
        public int NumChannels => Pointer != null ? Pointer->ch_layout.nb_channels : default;

        /// <summary>
        /// Gets the audio sample format.
        /// </summary>
        public SampleFormat SampleFormat => Pointer != null ? (SampleFormat)Pointer->format : SampleFormat.None;

        /// <summary>
        /// Gets the channel layout.
        /// </summary>
        internal AVChannelLayout ChannelLayout => Pointer != null ? Pointer->ch_layout : default;

        private bool IsPlanar =>
            SampleFormat is SampleFormat.SignedWordP or SampleFormat.SingleP or SampleFormat.SignedDWordP;

        /// <summary>
        /// Creates an audio frame with given dimensions and allocates a buffer for it.
        /// </summary>
        /// <param name="sample_rate">The sample rate of the audio frame.</param>
        /// <param name="num_channels">The number of channels in the audio frame.</param>
        /// <param name="num_samples">The number of samples in the audio frame.</param>
        /// <param name="channel_layout">The channel layout to be used by the audio frame.</param>
        /// <param name="sampleFormat">The audio sample format.</param>
        /// <param name="decodingTimestamp">The timestamp when the frame has to be decoded.</param>
        /// <param name="presentationTimestamp">The timestamp when the frame has to be presented.</param>
        /// <returns>The new audio frame.</returns>
        public static AudioFrame Create(int sample_rate, int num_channels, int num_samples, AVChannelLayout channel_layout, SampleFormat sampleFormat, long decodingTimestamp, long presentationTimestamp)
        {
            var frame = ffmpeg.av_frame_alloc();

            frame->sample_rate = sample_rate;

            frame->nb_samples = num_samples;
            frame->ch_layout = channel_layout;
            frame->format = (int)sampleFormat;

            frame->pts = presentationTimestamp;
            frame->pkt_dts = decodingTimestamp;

            ffmpeg.av_frame_get_buffer(frame, 32);

            return new AudioFrame(frame);
        }

        /// <summary>
        /// Creates an empty frame for decoding.
        /// </summary>
        /// <returns>The empty <see cref="AudioFrame"/>.</returns>
        public static AudioFrame CreateEmpty() => new AudioFrame();

        /// <summary>
        /// Fetches raw audio data from this audio frame for specified channel.
        /// </summary>
        /// <param name="channel">The index of audio channel that should be retrieved, allowed range: [0..<see cref="NumChannels"/>).</param>
        /// <returns>The span with samples in range of [-1.0, ..., 1.0].</returns>
        public Span<T> GetChannelData<T>(uint channel)
            where T : unmanaged
        {
            /*if (SampleFormat != SampleFormat.SingleP)
                throw new Exception("Cannot extract channel data from an AudioFrame with a SampleFormat not equal to SampleFormat.SingleP");*/
            return IsPlanar ? new Span<T>(Pointer->data[channel], NumSamples) : new Span<T>(Pointer->data[channel], NumSamples * NumChannels);
        }

        /// <summary>
        /// Copies raw multichannel audio data from this frame to a heap allocated array.
        /// </summary>
        /// <returns>
        /// The span with <see cref="NumChannels"/> rows and <see cref="NumSamples"/> columns;
        /// samples in range of [-1.0, ..., 1.0].
        /// </returns>
        public float[][] GetSampleData()
        {
            if (SampleFormat != SampleFormat.SingleP)
                throw new Exception("Cannot extract sample data from an AudioFrame with a SampleFormat not equal to SampleFormat.SingleP");

            var samples = new float[NumChannels][];

            for (uint ch = 0; ch < NumChannels; ch++)
            {
                samples[ch] = new float[NumSamples];

                var channelData = GetChannelData<float>(ch);
                var sampleData = new Span<float>(samples[ch], 0, NumSamples);

                channelData.CopyTo(sampleData);
            }

            return samples;
        }

        /// <summary>
        /// Updates the specified channel of this audio frame with the given sample data.
        /// </summary>
        /// <param name="samples">An array of samples with length <see cref="NumSamples"/>.</param>
        /// <param name="channel">The index of audio channel that should be updated, allowed range: [0..<see cref="NumChannels"/>).</param>
        public void UpdateChannelData(float[] samples, uint channel)
        {
            if (SampleFormat != SampleFormat.SingleP)
                throw new Exception("Cannot update channel data of an AudioFrame with a SampleFormat not equal to SampleFormat.SingleP");

            var frameData = GetChannelData<float>(channel);
            var sampleData = new Span<float>(samples, 0, NumSamples);

            sampleData.CopyTo(frameData);
        }

        /// <summary>
        /// Updates this audio frame with the specified multi-channel sample data.
        /// </summary>
        /// <param name="samples">
        /// A 2D jagged array of multi-channel sample data
        /// with <see cref="NumChannels"/> rows and <see cref="NumSamples"/> columns.
        /// </param>
        public void UpdateFromSampleData(float[][] samples)
        {
            if (SampleFormat != SampleFormat.SingleP)
                throw new Exception("Cannot update sample data of an AudioFrame with a SampleFormat not equal to SampleFormat.SingleP");

            for (uint ch = 0; ch < NumChannels; ch++)
            {
                var newData = new Span<float>(samples[ch], 0, NumSamples);
                var frameData = GetChannelData<float>(ch);
                newData.CopyTo(frameData);
            }
        }

        /// <summary>
        /// Updates this audio frame with the specified audio data.
        /// (<see cref="AudioData.NumSamples"/> and <see cref="AudioData.NumChannels"/>
        /// should match the respective values for this instance!).
        /// </summary>
        /// <param name="audioData">The audio data.</param>
        public void UpdateFromAudioData(AudioData audioData)
        {
            if (SampleFormat != SampleFormat.SingleP)
                throw new Exception("Cannot update data of an AudioFrame with a SampleFormat not equal to SampleFormat.SingleP");

            for (uint ch = 0; ch < NumChannels; ch++)
            {
                var newData = audioData.GetChannelData<float>(ch);
                var currData = GetChannelData<float>(ch);

                newData.CopyTo(currData);
            }
        }

        /// <inheritdoc/>
        internal override unsafe void Update(AVFrame* newFrame)
        {
            if (newFrame->GetMediaType() != MediaType.Audio)
            {
                throw new ArgumentException("The new frame doesn't contain audio data.");
            }

            base.Update(newFrame);
        }
    }
}
