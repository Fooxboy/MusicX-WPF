using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using FFMediaToolkit;
using FFMediaToolkit.Audio;
using FFMediaToolkit.Decoding;
using FFmpegInteropX;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services.Player.Playlists;
using MusicX.Services.Player.Sources;
using MusicX.Shared.Player;
using TagLib;
using TagLib.Id3v2;
using Wpf.Ui;
using File = System.IO.File;
using Tag = TagLib.Id3v2.Tag;

namespace MusicX.Services;

public class DownloaderService
{
    private readonly ConfigService configService;
    private readonly BoomService _boomService;
    private readonly ISnackbarService _snackbarService;
    private readonly VkService _vkService;

    private readonly MediaTranscoder _mediaTranscoder = new();
    private readonly HttpClient _httpClient = new HttpClient();

    public DownloaderService(ConfigService configService, BoomService boomService, ISnackbarService snackbarService, VkService vkService)
    {
        this.configService = configService;
        _boomService = boomService;
        _snackbarService = snackbarService;
        _vkService = vkService;

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
        
        var fileName = $"{audio.GetArtistsString()} - {audio.Title}";
        fileName = ReplaceSymbols(fileName) + ".mp3";
            
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

        if (audio.Data is BoomTrackData)
        {
            await using var stream = await _boomService.Client.GetStreamAsync(audio.Data.Url, cancellationToken);
            await using var fileStream = File.Create(fileDownloadPath);
            await stream.CopyToAsync(fileStream, cancellationToken);
            progress?.Report((TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)));
        }
        else
        {
            // blocked by FFmpegMediaSource working only with active media player
            // using var ffmpegMediaSource = await FFmpegMediaSource.CreateFromUriAsync(audio.Data.Url, new()
            // {
            //     FFmpegOptions =
            //     {
            //         ["http_persistent"] = "false"
            //     }
            // }).AsTask(cancellationToken);
            // var streamSource = ffmpegMediaSource.GetMediaStreamSource();

            var streamSource = MediaSourceBase.CreateFFMediaStreamSource(audio.Data.Url);

            var encodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
            
            var destinationFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(fileDownloadPath))
                .AsTask(cancellationToken);
            var destination = await destinationFolder.CreateFileAsync(Path.GetFileName(fileDownloadPath)).AsTask(cancellationToken);
            using var destinationStream = await destination.OpenAsync(FileAccessMode.ReadWrite).AsTask(cancellationToken);
            
            var prepareOp =
                await _mediaTranscoder.PrepareMediaStreamSourceTranscodeAsync(streamSource,
                    destinationStream, encodingProfile).AsTask(cancellationToken);

            if (!prepareOp.CanTranscode)
            {
                _snackbarService.ShowException("Упс!",
                    prepareOp.FailureReason == TranscodeFailureReason.CodecNotFound
                        ? "Кажется ваша система не поддерживает загрузку треков.. Попробуйте обновить Windows до последней версии."
                        : "Не удалось загрузить трек");
                return;
            }

            var transcodeOp = prepareOp.TranscodeAsync();

            var duration = streamSource.Duration;
            transcodeOp.Progress += (_, position) =>
            {
                progress?.Report((TimeSpan.FromSeconds(position), duration));
            };

            await transcodeOp.AsTask(cancellationToken);
        }

        await AddMetadataAsync(audio, fileDownloadPath, cancellationToken);
    }

    private string ReplaceSymbols(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars())).Replace('.', '_');
    }

    private async Task AddMetadataAsync(PlaylistTrack audio, string filePath, CancellationToken cancellationToken)
    {
        Tag.DefaultVersion = 3;
        Tag.ForceDefaultVersion = true;

        using var tfile = TagLib.File.Create(filePath);

        tfile.Tag.Title = audio.Title;
        tfile.Tag.AlbumArtists = audio.MainArtists.Select(b => b.Name).ToArray();
        tfile.Tag.Comment = "Загружено с помощью Music X. https://t.me/MusicXPlayer";
        tfile.Tag.Conductor = "Music X Player - https://t.me/MusicXPlayer";
        tfile.Tag.Copyright = "Music X Player - https://t.me/MusicXPlayer";
        if (audio.FeaturedArtists != null) 
            tfile.Tag.Performers = audio.FeaturedArtists.Select(b => b.Name).ToArray();

        if (audio.Data is VkTrackData { HasLyrics: true } vkTrackData)
        {
            var vkLyrics = await _vkService.GetLyrics($"{vkTrackData.Info.OwnerId}_{vkTrackData.Info.Id}");

            tfile.Tag.Lyrics = string.Join(Environment.NewLine,
                vkLyrics.LyricsInfo.Timestamps is { Count: > 0 }
                    ? vkLyrics.LyricsInfo.Timestamps.Select(b => b.Line)
                    : vkLyrics.LyricsInfo.Text);
        }

        if (audio.AlbumId != null)
        {
            tfile.Tag.Album = audio.AlbumId.Name;

            if (audio.AlbumId.CoverUrl != null)
            {
                var thumbData = await _httpClient.GetByteArrayAsync(audio.AlbumId.BigCoverUrl ?? audio.AlbumId.CoverUrl, cancellationToken);

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
        }
        
        tfile.Save();
    }

    public Task<bool> CheckExistAllDownloadTracksAsync()
    {
        return Task.FromResult(Directory.Exists(Path.Combine(GetDownloadDirectoryAsync(), "Музыка Вконтакте")));
    }
}