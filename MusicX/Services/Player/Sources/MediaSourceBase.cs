using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using FFMediaToolkit;
using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Sources;

public abstract class MediaSourceBase : ITrackMediaSource
{
    protected static MediaFile? CurrentSource { get; set; } // holds reference so it wont be collected before audio actually ends

    private TimeSpan _position;

    protected readonly MediaOptions MediaOptions = new()
    {
        StreamsToLoad = MediaMode.Audio,
        AudioSampleFormat = SampleFormat.SignedWord,
        DemuxerOptions =
        {
            FlagDiscardCorrupt = true,
            PrivateOptions =
            {
                ["http_persistent"] = "false"
            }
        }
    };

    public abstract Task<MediaPlaybackItem?> CreateMediaSourceAsync(MediaPlaybackSession playbackSession,
        PlaylistTrack track,
        CancellationToken cancellationToken = default);

    protected MediaPlaybackItem CreateMediaPlaybackItem(MediaFile file)
    {
        var properties =
            AudioEncodingProperties.CreatePcm((uint)file.Audio.Info.SampleRate, (uint)file.Audio.Info.NumChannels, 16);

        var streamingSource = new MediaStreamSource(new AudioStreamDescriptor(properties))
        {
            CanSeek = true,
            IsLive = true,
            Duration = file.Audio.Info.Duration,
            BufferTime = file.Audio.Info.Duration / 3
        };

        streamingSource.Starting += (_, args) =>
        {
            _position = args.Request.StartPosition == TimeSpan.Zero
                ? CurrentSource!.Info.StartTime
                : args.Request.StartPosition.GetValueOrDefault();
            
            args.Request.SetActualStartPosition(_position);
            
            try
            {
                if (CurrentSource == null) return;
                
                var position = _position;
                lock (CurrentSource)
                {
                    if (CurrentSource is null)
                        return;
                    CurrentSource.Audio.GetFrame(position);
                }
            }
            catch (FFmpegException)
            {
            }
        };

        streamingSource.SampleRequested += (_, args) =>
        {
            AudioData frame;
            while (true)
            {
                try
                {
                    if (CurrentSource is null)
                        return;

                    lock (CurrentSource)
                    {
                        if (CurrentSource is null)
                            return;
                        if (!CurrentSource.Audio.TryGetNextFrame(out frame))
                            return;
                    }

                    _position = CurrentSource.Audio.Position;
                    
                    break;
                }
                catch (FFmpegException)
                {
                }
            }

            var blockSize = frame.NumSamples * Unsafe.SizeOf<short>();
            var array = new byte[frame.NumChannels * blockSize];

            frame.GetChannelData<short>(0).CopyTo(MemoryMarshal.Cast<byte, short>(array));

            args.Request.Sample = MediaStreamSample.CreateFromBuffer(array.AsBuffer(), _position);
        };

        return new (MediaSource.CreateFromMediaStreamSource(streamingSource));
    }
}