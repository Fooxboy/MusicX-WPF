using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using FFMediaToolkit;
using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Encoding;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;
using TagLib;
using TagLib.Id3v2;
using AudioCodec = FFMediaToolkit.Encoding.AudioCodec;
using File = System.IO.File;
using Tag = TagLib.Id3v2.Tag;

namespace MusicX.Services;

public class DownloaderService
{
    private static readonly Semaphore FFmpegSemaphore = new(1, 1, "MusicX_FFmpegSemaphore");
    
    private readonly ConfigService configService;
    private readonly BoomService _boomService;

    public DownloaderService(ConfigService configService, BoomService boomService)
    {
        this.configService = configService;
        _boomService = boomService;

        FFmpegLoader.FFmpegPath = AppContext.BaseDirectory;
    }

    public string GetDownloadDirectoryAsync()
    {
        var directory = configService.Config.DownloadDirectory ??
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "MusicX");

        return Directory.CreateDirectory(directory).FullName;
    }

    public async Task DownloadAudioAsync(PlaylistTrack audio, IProgress<(TimeSpan Position, TimeSpan Duration)>? progress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(audio.Data.Url))
            return;
        
        var fileName = $"{audio.GetArtistsString()} - {audio.Title}.mp3";
        fileName = ReplaceSymbols(fileName);
            
        string fileDownloadPath;
        var musicFolder = GetDownloadDirectoryAsync();

        if (audio.Data is DownloaderData data)
        {
            var name = ReplaceSymbols(data.PlaylistName);

            var playlistDirPath = Path.Combine(musicFolder, name);

            if (!Directory.Exists(playlistDirPath))
            {
                Directory.CreateDirectory(playlistDirPath);
            }

            fileDownloadPath = Path.Combine(playlistDirPath, fileName);
        }
        else
        {
            fileDownloadPath = Path.Combine(musicFolder, fileName);
        }

        var i = 0;
        while (File.Exists(fileDownloadPath))
        {
            fileDownloadPath = fileDownloadPath.Replace(".mp3", string.Empty);
            var value = $"({i})";
            if (fileDownloadPath.EndsWith(value))
                fileDownloadPath = fileDownloadPath[..^value.Length];
            fileDownloadPath += $"({++i}).mp3";
        }

        var options = new MediaOptions
        {
            StreamsToLoad = MediaMode.Audio,
            DemuxerOptions =
            {
                FlagDiscardCorrupt = true,
                PrivateOptions =
                {
                    ["http_persistent"] = "false"
                }
            }
        };

        using var mediaFile = audio.Data is BoomTrackData ? MediaFile.Open(_boomService.Client.GetStreamAsync(audio.Data.Url, cancellationToken).Result, options) : MediaFile.Open(audio.Data.Url, options);

        using (var outputFile = MediaBuilder.CreateContainer(fileDownloadPath)
                   .WithAudio(new(mediaFile.Audio.Info.SampleRate, mediaFile.Audio.Info.NumChannels, AudioCodec.MP3)
                   {
                       Bitrate = 320_000,
                       SampleFormat = mediaFile.Audio.Info.SampleFormat,
                       SamplesPerFrame = mediaFile.Audio.Info.SamplesPerFrame
                   })
                   .UseMetadata(new()
                   {
                       Title = audio.Title,
                       Metadata =
                       {
                           ["artist"] = audio.MainArtists.First().Name,
                           ["comment"] = "Загружено с помощью Music X. https://t.me/MusicXPlayer",
                           ["conductor"] = "Music X Player"
                       },
                       Album = audio.AlbumId?.Name ?? "",
                       Copyright = "Music X Player - https://t.me/MusicXPlayer"
                   }).Create())
        {
            bool ProcessSample()
            {
                AudioData audioData;
                try
                {
                    if (!mediaFile.Audio.TryGetNextFrame(out audioData))
                        return false;
                }
                catch (FFmpegException)
                {
                    return true;
                }
                finally
                {
                    FFmpegSemaphore.Release();
                }

                progress?.Report((mediaFile.Audio.Position, audio.Data.Duration));

                outputFile.Audio.AddFrame(audioData);
                audioData.Dispose();
                return true;
            }

            do
            {
                await FFmpegSemaphore.WaitOneAsync(cancellationToken);
            } while (ProcessSample());
        }
        
        await AddMetadataAsync(audio, fileDownloadPath, cancellationToken);
    }

    private void Ffmpeg_Data(object sender, DataReceivedEventArgs e)
    {
        Debug.WriteLine(e.Data);
    }

    private string ReplaceSymbols(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }

    private async Task AddMetadataAsync(PlaylistTrack audio, string filePath, CancellationToken cancellationToken)
    {
        Tag.DefaultVersion = 3;
        Tag.ForceDefaultVersion = true;

        var tfile = TagLib.File.Create(filePath);
        
        // TODO: Stop using taglib

        if (audio.AlbumId != null)
        {
            using var httpClient = new HttpClient();
            var thumbData = await httpClient.GetByteArrayAsync(audio.AlbumId.BigCoverUrl ?? audio.AlbumId.CoverUrl, cancellationToken);

            var cover = new AttachedPictureFrame
            {
                Type = PictureType.FrontCover,
                Description = "Cover",
                MimeType = MediaTypeNames.Image.Jpeg,
                Data = thumbData,
                TextEncoding = StringType.UTF16
            };
            tfile.Tag.Pictures = new IPicture[] {cover};
        }
        
        tfile.Save();
    }

    public Task<bool> CheckExistAllDownloadTracksAsync()
    {
        return Task.FromResult(Directory.Exists(Path.Combine(GetDownloadDirectoryAsync(), "Музыка Вконтакте")));
    }
}