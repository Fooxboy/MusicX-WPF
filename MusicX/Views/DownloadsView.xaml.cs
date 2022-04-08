using DryIoc;
using MusicX.Controls;
using MusicX.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для DownloadsView.xaml
    /// </summary>
    public partial class DownloadsView : Page
    {

        private string ffmpegPath = $"{AppDomain.CurrentDomain.BaseDirectory}ffmpeg";
        private string ffmpegUrl = "https://fooxboy.blob.core.windows.net/musicx/ffmpeg.exe";

        public DownloadsView()
        {
            InitializeComponent();
            this.Loaded += DownloadsView_Loaded;
        }

        private DownloaderService downloaderService;

        private void DownloadsView_Loaded(object sender, RoutedEventArgs e)
        {
            if(StaticService.WebClient != null)
            {
                DownloadFfmpeg.Visibility = Visibility.Visible;

                StaticService.WebClient.DownloadFileCompleted += Client_DownloadFileCompleted;
                StaticService.WebClient.DownloadProgressChanged += Client_DownloadProgressChanged;
                return;
            }


            if(!File.Exists(ffmpegPath + "\\ffmpeg.exe"))
            {
                NoAvalible.Visibility = Visibility.Visible;

                return;
            }

            ContentGrid.Visibility = Visibility.Visible;

            this.downloaderService = StaticService.Container.Resolve<DownloaderService>();

            downloaderService.ChangeProgress += DownloaderService_ChangeProgress;
            downloaderService.StartedDownload += DownloaderService_StartedDownload;
            downloaderService.CompleteDownload += DownloaderService_CompleteDownload;
            downloaderService.AddedQueueItem += DownloaderService_AddedQueueItem;
            downloaderService.RemoveQueueItem += DownloaderService_RemoveQueueItem;


            if (downloaderService.QueueDownloads.Count > 0)
            {
                QueueDownloads.Visibility = Visibility.Visible;

                foreach(var audio in downloaderService.QueueDownloads)
                {
                    QueueTracks.Children.Add(new TrackControl() { Audio = audio, ShowCard = false });
                    QueueTracks.Children.Add(new Rectangle() { Height = 1, Fill = Brushes.White, Margin = new Thickness(5, 0, 5, 5), Opacity = 0.2 });


                }
            }

        }

        private void DownloaderService_RemoveQueueItem(Core.Models.Audio audio)
        {
            QueueTracks.Children.Clear();
            foreach (var audios in downloaderService.QueueDownloads)
            {
                QueueTracks.Children.Add(new TrackControl() { Audio = audios, ShowCard = false });
                QueueTracks.Children.Add(new Rectangle() { Height = 1, Fill = Brushes.White, Margin = new Thickness(5, 0, 5, 5), Opacity = 0.2 });

            }
        }

        private void DownloaderService_AddedQueueItem(Core.Models.Audio audio)
        {
            if(QueueDownloads.Visibility == Visibility.Collapsed)
            {
                QueueDownloads.Visibility = Visibility.Visible;
            }

            QueueTracks.Children.Add(new TrackControl() { Audio = audio, ShowCard = false });
        }

        private void DownloaderService_CompleteDownload(Core.Models.Audio audio)
        {
            NowDownload.Visibility = Visibility.Collapsed;
        }

        private void DownloaderService_StartedDownload(Core.Models.Audio audio)
        {
            NowDownload.Visibility = Visibility.Visible;

            if(audio.Album != null) NowDownloadCover.ImageSource = new BitmapImage(new Uri(audio.Album.Cover));

            NowDownloadTitle.Text = audio.Title;
            NowDownloadArtist.Text = audio.Artist;
        }

        private void DownloaderService_ChangeProgress(Core.Models.Audio audio, int kb)
        {

            if(NowDownload.Visibility == Visibility.Collapsed)
            {
                NowDownload.Visibility = Visibility.Visible;

                if (audio.Album != null) NowDownloadCover.ImageSource = new BitmapImage(new Uri(audio.Album.Cover));

                NowDownloadTitle.Text = audio.Title;
                NowDownloadArtist.Text = audio.Artist;
            }


            NowDownloadProgress.Maximum = audio.Duration * 39;
            NowDownloadProgress.Value = kb;

            if (kb > 1024)
            {
                NowDownloadValueKind.Text = "МБ";
                kb /= 1024;
                NowDownloadValue.Text = kb.ToString();
                
            }
            else
            {
                NowDownloadValueKind.Text = "КБ";
                NowDownloadValue.Text = kb.ToString();
            }
        }

       

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            NoAvalible.Visibility = Visibility.Collapsed;
            DownloadFfmpeg.Visibility = Visibility.Visible;

            await DownloadFfmpegAsync();
        }

        private async Task DownloadFfmpegAsync()
        {
            if(!Directory.Exists(ffmpegPath))
            {
                Directory.CreateDirectory(ffmpegPath);
            }

            using(var client = new WebClient())
            {
                StaticService.WebClient = client;
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                client.DownloadProgressChanged += Client_DownloadProgressChanged;

                await client.DownloadFileTaskAsync(ffmpegUrl, ffmpegPath += "\\ffmpeg.exe");
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.DonloadProgress.Maximum = e.TotalBytesToReceive;
                    this.DonloadProgress.Value = e.BytesReceived;

                    double left = e.TotalBytesToReceive - e.BytesReceived;

                    left = left / 1024;

                    if (left > 1024)
                    {
                        DownloadedKind.Text = "МБ";
                        left /= 1024;
                        DownloadedCount.Text = Math.Round(left, 2).ToString();
                    }
                    else
                    {
                        DownloadedKind.Text = "КБ";
                        DownloadedCount.Text = Math.Round(left, 2).ToString();
                    }

                });
            }
            catch (Exception ex)
            {
                var notifications = StaticService.Container.Resolve<Services.NotificationsService>();
                notifications.Show("Произошла ошибка", "Мы не смогли скачать дополнительный компонент, попробуйте перезапустить приложение.");

                var logger = StaticService.Container.Resolve<Logger>();

                logger.Error(ex, ex.Message);

            }
        }

        private void Client_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var notifications = StaticService.Container.Resolve<Services.NotificationsService>();
            notifications.Show("Загрузка завершена", "Дополнительный компонент был загружен. Теперь Вы можете скачивать музыку!");

            ContentGrid.Visibility = Visibility.Visible;
            DownloadFfmpeg.Visibility = Visibility.Collapsed;
            

            StaticService.WebClient = null;
        }

        private void ShowAudiosButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = downloaderService.musicFolder,
                UseShellExecute = true
            });
        }

        private void DownloadAllPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            var notifications = StaticService.Container.Resolve<Services.NotificationsService>();

            notifications.Show("Временно недоступно", "Загрузка всех плейлистов временно недоступно, но в следующем обновлении обязательно заработает!");

        }

        private void DownloadAllTracksButton_Click(object sender, RoutedEventArgs e)
        {
            var notifications = StaticService.Container.Resolve<Services.NotificationsService>();


            notifications.Show("Временно недоступно", "Загрузка всех треков временно недоступна, но в следующем обновлении обязательно заработает!");

        }
    }
}
