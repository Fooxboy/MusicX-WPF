using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AppCenter.Analytics;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Controls;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.ViewModels;
using NLog;
using Wpf.Ui;

namespace MusicX.Views;

/// <summary>
/// Логика взаимодействия для DownloadsView.xaml
/// </summary>
public partial class DownloadsView : Page, IMenuPage
{
    private long maxTotal;

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
        
        var connectionService = StaticService.Container.GetRequiredService<BackendConnectionService>();
        connectionService.ReportMetric("OpenDownloads");
            
        ContentGrid.Visibility = Visibility.Visible;
    }
    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        NoAvailable.Visibility = Visibility.Collapsed;
        DownloadFfmpeg.Visibility = Visibility.Visible;
    }

    private void Client_DownloadProgressChanged((long TotalBytes, long Bytes) valueTuple)
    {
        try
        {
            var (totalBytes, downloadedBytes) = valueTuple;
            
            if (totalBytes >= maxTotal)
                maxTotal = totalBytes;
            else
                return;
            
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                this.DonloadProgress.Maximum = totalBytes;
                this.DonloadProgress.Value = downloadedBytes;

                double left = totalBytes - downloadedBytes;

                left /= 1024;

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
            snackbarService.ShowException("Произошла ошибка",
                "Мы не смогли скачать дополнительный компонент, попробуйте перезапустить приложение.");

            var logger = StaticService.Container.GetRequiredService<Logger>();

            logger.Error(ex, ex.Message);

        }
    }
    public string MenuTag { get; set; }
}