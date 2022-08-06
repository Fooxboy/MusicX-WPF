using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using MusicX.Core.Models;
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

    public DownloaderService(ConfigService configService)
    {
        this.configService = configService;
        FFmpeg.SetExecutablesPath(ffmpegPath);
    }

    public async Task<string> GetDownloadDirectoryAsync()
    {
        var directory = (await configService.GetConfig()).DownloadDirectory ??
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "MusicX");

        return Directory.CreateDirectory(directory).FullName;
    }

    public async Task DownloadAudioAsync(Audio audio, IProgress<ConversionProgressEventArgs>? progress = null, CancellationToken cancellationToken = default)
    {
        var fileName = $"{audio.Artist} - {audio.Title}.mp3";
        fileName = ReplaceSymbols(fileName);
            
        string fileDownloadPath;
        var musicFolder = await GetDownloadDirectoryAsync();

        if (audio.DownloadPlaylistName != null)
        {
            audio.DownloadPlaylistName = ReplaceSymbols(audio.DownloadPlaylistName);

            var playlistDirPath = Path.Combine(musicFolder, audio.DownloadPlaylistName);

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

        var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(audio.Url, fileDownloadPath);

        conversion.OnDataReceived += Ffmpeg_Data;
        if (progress is not null)
            conversion.OnProgress += (_, args) => progress.Report(args);

        await conversion.SetOutputFormat(Format.mp3).AddParameter("-http_persistent false").Start(cancellationToken);
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

    private async Task AddMetadataAsync(Audio audio, string filePath, CancellationToken cancellationToken)
    {
        Tag.DefaultVersion = 3;
        Tag.ForceDefaultVersion = true;

        var tfile = TagLib.File.Create(filePath);

        tfile.Tag.Title = audio.Title;
        tfile.Tag.Performers = new[] {audio.Artist};
        tfile.Tag.AlbumArtists = new[] {audio.Artist};

        if (audio.Album != null)
        {
            tfile.Tag.Album = audio.Album.Title;
            tfile.Tag.Year = 2022;

            using var httpClient = new HttpClient();
            var thumbData = await httpClient.GetByteArrayAsync(audio.Album.Thumb.Photo600, cancellationToken);

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
        tfile.Tag.MusicIpId = audio.OwnerId + "_" + audio.Id + "_" + audio.OwnerId;

        tfile.Tag.Conductor = "Music X Player";

        tfile.Save();
    }

    public async Task<bool> CheckExistAllDownloadTracksAsync()
    {
        return Directory.Exists(Path.Combine(await GetDownloadDirectoryAsync(), "Музыка Вконтакте"));
    }
}