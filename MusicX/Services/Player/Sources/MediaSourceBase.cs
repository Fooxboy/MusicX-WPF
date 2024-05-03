using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using FFMediaToolkit;
using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using FFmpegInteropX;
using MusicX.Helpers;
using MusicX.Shared.Player;
using WinRT;

namespace MusicX.Services.Player.Sources;

public abstract class MediaSourceBase : ITrackMediaSource
{
    private static readonly Semaphore FFmpegSemaphore = new(1, 1, "MusicX_FFmpegSemaphore");
    
    protected static readonly MediaOptions MediaOptions = new()
    {
        StreamsToLoad = MediaMode.Audio,
        AudioSampleFormat = SampleFormat.SignedWord,
        DemuxerOptions =
        {
            FlagDiscardCorrupt = true,
            FlagEnableFastSeek = true,
            SeekToAny = true,
            PrivateOptions =
            {
                ["http_persistent"] = "false",
                ["reconnect"] = "1",
                ["reconnect_streamed"] = "1",
                ["reconnect_on_network_error"] = "1",
                ["reconnect_delay_max"] = "30",
                ["reconnect_on_http_error"] = "4xx,5xx",
                ["stimeout"] = "30000000",
                ["timeout"] = "30000000",
                ["rw_timeout"] = "30000000"
            }
        }
    };

    public abstract Task<bool> OpenWithMediaPlayerAsync(MediaPlayer player, PlaylistTrack track, CancellationToken cancellationToken = default);

    protected static MediaPlaybackItem CreateMediaPlaybackItem(MediaFile file)
    {
        var streamingSource = CreateFFMediaStreamSource(file);

        return new (MediaSource.CreateFromMediaStreamSource(streamingSource));
    }

    public static MediaStreamSource CreateFFMediaStreamSource(string url)
    {
        return CreateFFMediaStreamSource(MediaFile.Open(url, MediaOptions));
    }

    public static MediaStreamSource CreateFFMediaStreamSource(MediaFile file)
    {
        var properties =
            AudioEncodingProperties.CreatePcm((uint)file.Audio.Info.SampleRate, (uint)file.Audio.Info.NumChannels, 16);

        var streamingSource = new MediaStreamSource(new AudioStreamDescriptor(properties))
        {
            CanSeek = true,
            IsLive = true,
            Duration = file.Audio.Info.Duration,
            BufferTime = TimeSpan.Zero
        };
        
        var position = TimeSpan.Zero;

        streamingSource.Starting += (_, args) =>
        {
            position = args.Request.StartPosition == TimeSpan.Zero
                ? file.Info.StartTime
                : args.Request.StartPosition.GetValueOrDefault();
            
            args.Request.SetActualStartPosition(position);

            try
            {
                FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(10));
                file.Audio.GetFrame(position);
            }
            catch (FFmpegException)
            {
            }
            finally
            {
                FFmpegSemaphore.Release();
            }
        };

        streamingSource.Closed += (_, _) =>
        {
            FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(10));
            try
            {
                file.Dispose();
            }
            finally
            {
                FFmpegSemaphore.Release();
            }
        };

        streamingSource.SampleRequested += (_, args) =>
        {
            FFmpegSemaphore.WaitOne(TimeSpan.FromSeconds(10));
            
            try
            {
                if (file.IsDisposed)
                    return;
                
                var array = ProcessSample();
                if (array != null)
                    args.Request.Sample = MediaStreamSample.CreateFromBuffer(array.AsBuffer(), position);
            }
            finally
            {
                FFmpegSemaphore.Release();
            }
        };
        
        byte[]? ProcessSample()
        {
            AudioData frame;
            while (true)
            {
                try
                {
                    if (!file.Audio.TryGetNextFrame(out frame))
                        return null;

                    position = file.Audio.Position;
                    
                    break;
                }
                catch (FFmpegException)
                {
                }
            }

            var blockSize = frame.NumSamples * Unsafe.SizeOf<short>();
            var array = new byte[frame.NumChannels * blockSize];

            frame.GetChannelData<short>(0).CopyTo(MemoryMarshal.Cast<byte, short>(array));
            
            frame.Dispose();

            return array;
        }

        return streamingSource;
    }

    public static Task<FFmpegMediaSource> CreateWinRtMediaSource(TrackData data, IReadOnlyDictionary<string, string>? customOptions = null, CancellationToken cancellationToken = default)
    {
        var options = new PropertySet();
        
        foreach (var option in MediaOptions.DemuxerOptions.PrivateOptions)
        {
            options.Add(option.Key, option.Value);
        }
        
        if (customOptions != null)
            foreach (var (key, value) in customOptions)
                options[key] = value;

        return FFmpegMediaSource.CreateFromUriAsync(data.Url, new()
        {
            FFmpegOptions = options,
            General =
            {
                ReadAheadBufferEnabled = true,
                SkipErrors = uint.MaxValue,
                KeepMetadataOnMediaSourceClosed = false
            }
        }).AsTask(cancellationToken);
    }

    protected static void RegisterSourceObjectReference(MediaPlayer player, IWinRTObject rtObject)
    {
        GC.SuppressFinalize(rtObject.NativeObject);
        
        player.SourceChanged += PlayerOnSourceChanged;

        void PlayerOnSourceChanged(MediaPlayer sender, object args)
        {
            player.SourceChanged -= PlayerOnSourceChanged;
            
            if (rtObject is IDisposable disposable)
                disposable.Dispose();
            else
                GC.ReRegisterForFinalize(rtObject);
        }
    }
}