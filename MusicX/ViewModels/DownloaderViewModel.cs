using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Analytics;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player.Playlists;
using NLog;
using Wpf.Ui.Common;
using Xabe.FFmpeg.Events;
namespace MusicX.ViewModels;

public class DownloaderViewModel : BaseViewModel
{
    public ObservableRangeCollection<PlaylistTrack> DownloadQueue { get; } = new();
    public PlaylistTrack? CurrentDownloadingAudio { get; set; }
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
        QueueAllMyTracksCommand = new AsyncCommand(QueueAllMyTracks);
        QueueAllMyPlaylistsCommand = new AsyncCommand(QueueAllMyPlaylists);
        OpenMusicFolderCommand = new AsyncCommand(OpenMusicFolder);
    }

    public async ValueTask AddPlaylistToQueueAsync(IEnumerable<PlaylistTrack> tracks, string title)
    {
        tracks = tracks.Select(b => b with { Data = new DownloaderData(b.Data.Url, 
                                                                       b.Data.IsLiked, 
                                                                       b.Data.IsExplicit, 
                                                                       b.Data.Duration,
                                                                       title)});
        
        if (Application.Current.Dispatcher.CheckAccess())
            DownloadQueue.AddRange(tracks, NotifyCollectionChangedAction.Reset);
        else
            await Application.Current.Dispatcher.InvokeAsync(() => DownloadQueue.AddRange(tracks));
        notificationsService.Show("Скачивание начато", $"{title} добавлен в очередь");
    }
    public async Task AddPlaylistToQueueAsync(long playlistId, long ownerId, string accessKey)
    {
        var playlist = await vkService.LoadFullPlaylistAsync(playlistId, ownerId, accessKey);
        await AddPlaylistToQueueAsync(playlist.Audios.Select(TrackExtensions.ToTrack), playlist.Playlist.Title!);
    }

    private async Task OpenMusicFolder()
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

    private async Task QueueAllMyTracks()
    {
        if (await downloaderService.CheckExistAllDownloadTracksAsync()) return;
        var tracks = new List<Audio>();

        while (true)
        {
            var tr = await vkService.AudioGetAsync(null, null, null, tracks.Count);

            tracks.AddRange(tr.Items);

            if (tr.Items.Count < 100) break;
        }

        await AddPlaylistToQueueAsync(tracks.Select(TrackExtensions.ToTrack), "Музыка ВКонтакте");
        StartDownloading();
    }

    private async Task QueueAllMyPlaylists()
    {
        var config = await configService.GetConfig();
            
        var playlists = await vkService.GetPlaylistsAsync(config.UserId);

        foreach (var playlist in playlists)
        {
            var tracks = await vkService.LoadFullPlaylistAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey);

            await AddPlaylistToQueueAsync(tracks.Items.Select(TrackExtensions.ToTrack), playlist.Title!);
        }
        
        StartDownloading();
    }

    private async void StartDownloading()
    {

        var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
        Analytics.TrackEvent("Download Track", properties);

        if (IsDownloading)
            return;
        tokenSource = new();
        await Task.Run(() => DownloaderTask(tokenSource.Token));
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
