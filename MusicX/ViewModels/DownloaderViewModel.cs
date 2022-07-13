using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using WPFUI.Common;
using Xabe.FFmpeg.Events;
namespace MusicX.ViewModels;

public class DownloaderViewModel : BaseViewModel
{
    public ObservableCollection<Audio> DownloadQueue { get; } = new();
    public Audio? CurrentDownloadingAudio { get; set; }
    public int DownloadProgress { get; set; }
    public bool IsDownloading { get; set; }
    
    public ICommand StartDownloadingCommand { get; }
    public ICommand StopDownloadingCommand { get; }
    public ICommand ClearQueueCommand { get; }
    public ICommand QueueAllMyTracksCommand { get; }
    public ICommand QueueAllMyPlaylistsCommand { get; }
    public ICommand OpenMusicFolderCommand { get; }

    private CancellationTokenSource? tokenSource;
    private readonly DownloaderService downloaderService;
    private readonly NotificationsService notificationsService;
    private readonly Logger logger;
    private readonly VkService vkService;
    private readonly ConfigService configService;
    private readonly IProgress<ConversionProgressEventArgs> progress;

    public DownloaderViewModel(DownloaderService downloaderService, NotificationsService notificationsService, Logger logger, VkService vkService, ConfigService configService)
    {
        this.downloaderService = downloaderService;
        this.notificationsService = notificationsService;
        this.logger = logger;
        this.vkService = vkService;
        this.configService = configService;
        progress = new Progress<ConversionProgressEventArgs>(args => DownloadProgress = args.Percent);

        StartDownloadingCommand = new RelayCommand(StartDownloading);
        StopDownloadingCommand = new RelayCommand(StopDownloading);
        ClearQueueCommand = new RelayCommand(ClearQueue);
        QueueAllMyTracksCommand = new RelayCommand(QueueAllMyTracks);
        QueueAllMyPlaylistsCommand = new RelayCommand(QueueAllMyPlaylists);
        OpenMusicFolderCommand = new RelayCommand(OpenMusicFolder);
    }

    public void AddPlaylistToQueue(IEnumerable<Audio> tracks, string title)
    {
        foreach (var track in tracks)
        {
            track.DownloadPlaylistName = title;
            DownloadQueue.Add(track);
        }
    }

    private async void OpenMusicFolder()
    {
        var config = await configService.GetConfig();
        
        if (string.IsNullOrEmpty(config.DownloadDirectory) || !Directory.Exists(config.DownloadDirectory))
            return;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = config.DownloadDirectory,
            UseShellExecute = true
        });
    }

    private async void QueueAllMyTracks()
    {
        if (await downloaderService.CheckExistAllDownloadTracksAsync()) return;
        var tracks = new List<Audio>();

        while (true)
        {
            var tr = await vkService.AudioGetAsync(null, null, null, tracks.Count);

            tracks.AddRange(tr.Items);

            if (tr.Items.Count < 100) break;
        }

        AddPlaylistToQueue(tracks, "Музыка ВКонтакте");
        StartDownloading();
    }

    private async void QueueAllMyPlaylists()
    {
        var config = await configService.GetConfig();
            
        var playlists = await vkService.GetPlaylistsAsync(config.UserId);

        foreach (var playlist in playlists)
        {
            var tracks = await vkService.AudioGetAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey);

            AddPlaylistToQueue(tracks.Items, playlist.Title!);
        }
        
        StartDownloading();
    }

    private void StartDownloading()
    {
        if (!IsDownloading)
        {
            tokenSource = new();
            Task.Run(() => DownloaderTask(tokenSource.Token), tokenSource.Token);
        }
    }

    private void StopDownloading()
    {
        tokenSource?.Cancel();
        tokenSource?.Dispose();
        tokenSource = null;
    }

    private void ClearQueue()
    {
        if (IsDownloading)
            StopDownloading();

        DownloadQueue.Clear();
    }

    private async Task DownloaderTask(CancellationToken token)
    {
        IsDownloading = true;
        
        while (!token.IsCancellationRequested && DownloadQueue.Count > 0)
        {
            var audio = DownloadQueue[0];

            try
            {
                CurrentDownloadingAudio = audio;
                await downloaderService.DownloadAudioAsync(audio, progress, token);
                await Application.Current.Dispatcher.InvokeAsync(() => DownloadQueue.Remove(audio));
                DownloadProgress = 0;
            }
            catch (OperationCanceledException)
            {
                notificationsService.Show("Скачивание прервано", "Скачивание очереди было прервано");
            }
            catch (Exception e)
            {
                logger.Error(e);
                notificationsService.Show("Ошибка загрузки", "Мы не смогли загрузить трек");
            }
        }

        CurrentDownloadingAudio = null;
        DownloadProgress = 0;
        IsDownloading = false;
    }
}
