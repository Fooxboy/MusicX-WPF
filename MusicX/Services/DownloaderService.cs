using FFmpeg.NET;
using MusicX.Core.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MusicX.Services
{
    public class DownloaderService
    {
        public event ProgressDelegate ChangeProgress;
        public event CompleteDownloadDelegate CompleteDownload;
        public event StartedDownloadDelegate StartedDownload;
        public event StartedDownloadDelegate AddedQueueItem;
        public event StartedDownloadDelegate RemoveQueueItem;

        public delegate void ProgressDelegate(Audio audio, int kb);
        public delegate void CompleteDownloadDelegate(Audio audio);
        public delegate void StartedDownloadDelegate(Audio audio);


        public Audio CurrentDownload = null;

        public List<Audio> QueueDownloads = new List<Audio>();

        private string ffmpegPath = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg\\ffmpeg.exe";
        public string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\MusicX";

        private readonly Logger logger;
        private readonly NotificationsService notificationsService;

        private Engine ffmpeg;

        public DownloaderService(Logger logger, NotificationsService notificationsService)
        {
            this.logger = logger;
            this.notificationsService = notificationsService;
        }

        public async Task AddToQueueAsync(List<Audio> audios, string name)
        {
            if (!File.Exists(ffmpegPath))
            {
                throw new FileNotFoundException();
            }

            if (this.ffmpeg == null)
            {
                this.ffmpeg = new Engine(ffmpegPath);

                ffmpeg.Progress += Ffmpeg_Progress;
                ffmpeg.Error += Ffmpeg_Error;
                ffmpeg.Complete += Ffmpeg_Complete;
                ffmpeg.Data += Ffmpeg_Data;
            }

            try
            {
                if (!Directory.Exists(musicFolder))
                {
                    Directory.CreateDirectory(musicFolder);
                }

                foreach (var audio in audios)
                {
                    audio.DownloadPlaylistName = name;

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        AddedQueueItem?.Invoke(audio);
                    });

                }

                QueueDownloads.AddRange(audios);

                notificationsService.Show("Начало загрузки...", $"Треки плейлиста {name} добавлены в очередь");


                await TryDownloadAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                notificationsService.Show("Ошибка загрузки", "Мы не смогли загрузить трек");
            }
        } 

        public async Task AddToQueueAsync(Audio audio)
        {
            if(!File.Exists(ffmpegPath))
            {
                throw new FileNotFoundException();
            }

            if(this.ffmpeg == null)
            {
                this.ffmpeg = new Engine(ffmpegPath);

                ffmpeg.Progress += Ffmpeg_Progress;
                ffmpeg.Error += Ffmpeg_Error;
                ffmpeg.Complete += Ffmpeg_Complete;
                ffmpeg.Data += Ffmpeg_Data;
            }

            try
            {
                if(!Directory.Exists(musicFolder))
                {
                    Directory.CreateDirectory(musicFolder);
                }

                QueueDownloads.Add(audio);

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    AddedQueueItem?.Invoke(audio);
                });

                notificationsService.Show("Старт загрузки...", $"Трек {audio.Artist} - {audio.Title} добавлен в очередь.");


                await TryDownloadAsync();
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
                notificationsService.Show("Ошибка загрузки", "Мы не смогли загрузить трек");
            }
        }


        private async Task TryDownloadAsync()
        {
            try
            {
                if (CurrentDownload == null && QueueDownloads.Count > 0)
                {
                    CurrentDownload = QueueDownloads.FirstOrDefault();

                    if (CurrentDownload == null) return;

                    QueueDownloads.Remove(CurrentDownload);

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        RemoveQueueItem?.Invoke(CurrentDownload);
                    });

                    await this.DownloadAudioAsync(CurrentDownload);
                }
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
                notificationsService.Show("Ошибка загрузки", "Мы не смогли загрузить трек");
            }
           
        }

        private async Task DownloadAudioAsync(Audio audio)
        {
            try
            {
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    StartedDownload?.Invoke(audio);
                });
                notificationsService.Show("Загрузка трека", $"Трек {audio.Artist} - {audio.Title} сейчас загружается.");

                var fileName = $"{audio.Artist} - {audio.Title}.mp3";


                var folderDownload = string.Empty;

                if(audio.DownloadPlaylistName != null)
                {

                    audio.DownloadPlaylistName = ReplaceSymbols(audio.DownloadPlaylistName);

                    fileName= ReplaceSymbols(fileName);

                    if (!Directory.Exists($"{musicFolder}\\{audio.DownloadPlaylistName}"))
                    {
                        Directory.CreateDirectory($"{musicFolder}\\{audio.DownloadPlaylistName}");
                    }

                    folderDownload = $"{musicFolder}\\{audio.DownloadPlaylistName}\\{fileName}";
                }else
                {

                    fileName = ReplaceSymbols(fileName);



                    folderDownload = $"{musicFolder}\\{fileName}";
                }


                await ffmpeg.ExecuteAsync($"-http_persistent false -i \"{audio.Url}\" -c copy \"{folderDownload}\"", CancellationToken.None);

            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
                notificationsService.Show("Ошибка загрузки", "Мы не смогли загрузить трек");
            }
        }

        private void Ffmpeg_Data(object? sender, FFmpeg.NET.Events.ConversionDataEventArgs e)
        {

            Debug.WriteLine(e.Data);

            //throw new NotImplementedException();
        }

        private string ReplaceSymbols(string text)
        {
            if (text.Contains(':')) text = text.Replace(':', ' ');
            if (text.Contains('/')) text = text.Replace('/', ' ');
            if (text.Contains('\\')) text = text.Replace('\\', ' ');
            if (text.Contains('*')) text = text.Replace('*', ' ');
            if (text.Contains('«')) text = text.Replace('«', ' ');
            if (text.Contains('<')) text = text.Replace('<', ' ');
            if (text.Contains('>')) text = text.Replace('>', ' ');
            if (text.Contains('|')) text = text.Replace('|', ' ');
            if (text.Contains('%')) text = text.Replace('%', ' ');
            if (text.Contains('\"')) text = text.Replace('\"', ' ');
            if (text.Contains('@')) text = text.Replace('@', ' ');
            if (text.Contains('№')) text = text.Replace('№', ' ');

            return text;
        }

        private async void Ffmpeg_Complete(object? sender, FFmpeg.NET.Events.ConversionCompleteEventArgs e)
        {
            var fileName = $"{CurrentDownload.Artist} - {CurrentDownload.Title}.mp3";

            fileName = ReplaceSymbols(fileName);

            var pathFile = string.Empty;

            if(CurrentDownload.DownloadPlaylistName != null)
            {
                pathFile = $"{musicFolder}\\{ReplaceSymbols(CurrentDownload.DownloadPlaylistName)}";
            }else
            {
                pathFile = musicFolder;

            }

            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;

            var tfile = TagLib.File.Create($"{pathFile}\\{fileName}");

            tfile.Tag.Title = CurrentDownload.Title;
            tfile.Tag.Artists = new string[] { CurrentDownload.Artist };
            tfile.Tag.AlbumArtists = new string[] {CurrentDownload.Artist};

            var r = new Random().Next(0, int.MaxValue);

            if (CurrentDownload.Album != null)
            {
                tfile.Tag.Album = CurrentDownload.Album.Title;
                tfile.Tag.Year = 2022;



                using(var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(CurrentDownload.Album.Thumb.Photo600, musicFolder + $"\\{r}.jpg");
                }

                byte[] imageBytes;

                using (WebClient client = new WebClient())
                {
                    imageBytes = client.DownloadData(CurrentDownload.Album.Thumb.Photo600);
                }

                TagLib.Id3v2.AttachedPictureFrame cover = new TagLib.Id3v2.AttachedPictureFrame
                {
                    Type = TagLib.PictureType.FrontCover,
                    Description = "Cover",
                    MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                    Data = imageBytes,
                    TextEncoding = TagLib.StringType.UTF16
                };
                tfile.Tag.Pictures = new TagLib.IPicture[] { cover };
            }

            tfile.Tag.Comment = "Загружено с помощью Music X. https://t.me/MusicXPlayer";
            tfile.Tag.Copyright = "Music X Player - https://t.me/MusicXPlayer";
            tfile.Tag.MusicIpId = CurrentDownload.OwnerId + "_" + CurrentDownload.Id + "_" + CurrentDownload.OwnerId;

            tfile.Tag.Conductor = "Music X Player";

            tfile.Save();


            if(CurrentDownload.Album != null)
            {
                File.Delete(musicFolder + $"\\{r}.jpg");
            }


            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                CompleteDownload?.Invoke(CurrentDownload);
                notificationsService.Show("Трек загружен", $"{CurrentDownload.Artist} - {CurrentDownload.Title} был загружен в библиотеку.");
            });

            CurrentDownload = null;

            await TryDownloadAsync();
        }

        private async void Ffmpeg_Error(object? sender, FFmpeg.NET.Events.ConversionErrorEventArgs e)
        {
            CurrentDownload = null;

            notificationsService.Show("Ошибка загрузки", "Мы не смогли загрузить трек");

            await TryDownloadAsync();
            //throw new NotImplementedException();
        }

        private void Ffmpeg_Progress(object? sender, FFmpeg.NET.Events.ConversionProgressEventArgs e)
        {
            Debug.WriteLine(e.SizeKb + "KB");

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if(e.SizeKb != null) ChangeProgress?.Invoke(CurrentDownload, e.SizeKb.Value);
            });
        }
    }
}
