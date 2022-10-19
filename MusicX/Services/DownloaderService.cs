using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;
using TagLib;
using TagLib.Id3v2;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;
using File = System.IO.File;
using Tag = TagLib.Id3v2.Tag;

namespace MusicX.Services;

public class DownloaderService
{

    private readonly string ffmpegPath = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg";

    private readonly ConfigService configService;
    private readonly BoomService _boomService;

    public DownloaderService(ConfigService configService, BoomService boomService)
    {
        this.configService = configService;
        _boomService = boomService;
        FFmpeg.SetExecutablesPath(ffmpegPath);
    }

    public async Task<string> GetDownloadDirectoryAsync()
    {
        var directory = (await configService.GetConfig()).DownloadDirectory ??
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "MusicX");

        return Directory.CreateDirectory(directory).FullName;
    }

    public async Task DownloadAudioAsync(PlaylistTrack audio, IProgress<ConversionProgressEventArgs>? progress = null, CancellationToken cancellationToken = default)
    {
        var fileName = $"{audio.GetArtistsString()} - {audio.Title}.mp3";
        fileName = ReplaceSymbols(fileName);
            
        string fileDownloadPath;
        var musicFolder = await GetDownloadDirectoryAsync();

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

        if (File.Exists(fileDownloadPath))
            File.Delete(fileDownloadPath);

        var conversion = FFmpeg.Conversions.New()
                               .SetOutput(fileDownloadPath)
                               .AddParameter($"-i {audio.Data.Url}");

        conversion.OnDataReceived += Ffmpeg_Data;
        if (progress is not null)
            conversion.OnProgress += (_, args) => progress.Report(args);

        conversion.AddParameter(
            audio.Data is VkTrackData
                ? "-http_persistent false"
                : $"-headers \"Authorization: {_boomService.Client.DefaultRequestHeaders.Authorization}\"",
            ParameterPosition.PreInput);

        await conversion.Start(cancellationToken);
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

        tfile.Tag.Title = audio.Title;
        tfile.Tag.Performers = audio.MainArtists.Concat(audio.FeaturedArtists).Select(b => b.Name).ToArray();
        tfile.Tag.AlbumArtists = audio.MainArtists.Select(b => b.Name).ToArray();

        if (audio.AlbumId != null)
        {
            tfile.Tag.Album = audio.AlbumId.Name;
            tfile.Tag.Year = 2022;

            using var httpClient = new HttpClient();
            var thumbData = await httpClient.GetByteArrayAsync(audio.AlbumId.CoverUrl, cancellationToken);

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

        tfile.Tag.Comment = "Загружено с помощью Music X. https://t.me/MusicXPlayer";
        tfile.Tag.Copyright = "Music X Player - https://t.me/MusicXPlayer";
        if (audio.Data is VkTrackData data)
            tfile.Tag.MusicIpId = data.Info.OwnerId + "_" + data.Info.Id + "_" + data.Info.OwnerId;

        tfile.Tag.Conductor = "Music X Player";

        tfile.Save();
    }

    public async Task<bool> CheckExistAllDownloadTracksAsync()
    {
        return Directory.Exists(Path.Combine(await GetDownloadDirectoryAsync(), "Музыка Вконтакте"));
    }
}