using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AppCenter.Analytics;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Controls;
using MusicX.Services;
using MusicX.ViewModels;
using NLog;
using Wpf.Ui.Contracts;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace MusicX.Views;

/// <summary>
/// Логика взаимодействия для DownloadsView.xaml
/// </summary>
public partial class DownloadsView : Page, IMenuPage
{
    private long maxTotal;
    private string ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg");

    public DownloadsView()
    {
        InitializeComponent();
        DataContext = StaticService.Container.GetRequiredService<DownloaderViewModel>();
        this.Loaded += DownloadsView_Loaded;
    }

    private void DownloadsView_Loaded(object sender, RoutedEventArgs e)
    {
        var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
        Analytics.TrackEvent("OpenDownloads", properties);

        if (!File.Exists(Path.Combine(ffmpegPath, "version.json")))
        {
            NoAvailable.Visibility = Visibility.Visible;

            return;
        }
            
        ContentGrid.Visibility = Visibility.Visible;
    }
    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        NoAvailable.Visibility = Visibility.Collapsed;
        DownloadFfmpeg.Visibility = Visibility.Visible;

        await DownloadFfmpegAsync();
    }

    private async Task DownloadFfmpegAsync()
    {
        if(!Directory.Exists(ffmpegPath))
        {
            Directory.CreateDirectory(ffmpegPath);
        }
        else
        {
            foreach (var file in Directory.GetFiles(ffmpegPath))
            {
                File.Delete(file);
            }
        }

        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath, new Progress<ProgressInfo>(Client_DownloadProgressChanged));

        var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
        snackbarService.Show("Загрузка завершена",
            "Дополнительный компонент был загружен. Теперь Вы можете скачивать музыку!");

        ContentGrid.Visibility = Visibility.Visible;
        DownloadFfmpeg.Visibility = Visibility.Collapsed;
    }

    private void Client_DownloadProgressChanged(ProgressInfo progressInfo)
    {
        try
        {
            if (progressInfo.TotalBytes >= maxTotal)
                maxTotal = progressInfo.TotalBytes;
            else
                return;
            
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                this.DonloadProgress.Maximum = progressInfo.TotalBytes;
                this.DonloadProgress.Value = progressInfo.DownloadedBytes;

                double left = progressInfo.TotalBytes - progressInfo.DownloadedBytes;

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
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
            snackbarService.Show("Произошла ошибка",
                "Мы не смогли скачать дополнительный компонент, попробуйте перезапустить приложение.");

            var logger = StaticService.Container.GetRequiredService<Logger>();

            logger.Error(ex, ex.Message);

        }
    }
    public string MenuTag { get; set; }
}