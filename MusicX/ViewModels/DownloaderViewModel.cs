using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;
using NLog;
using Wpf.Ui;
using Wpf.Ui.Common;

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
    private readonly ISnackbarService _snackbarService;
    private readonly Logger logger;
    private readonly VkService vkService;
    private readonly ConfigService configService;
    private readonly IProgress<(TimeSpan Position, TimeSpan Duration)>? progress;

    public DownloaderViewModel(DownloaderService downloaderService, ISnackbarService snackbarService, Logger logger,
        VkService vkService, ConfigService configService)
    {
        this.downloaderService = downloaderService;
        _snackbarService = snackbarService;
        this.logger = logger;
        this.vkService = vkService;
        this.configService = configService;
        progress = new Progress<(TimeSpan Position, TimeSpan Duration)>(args => DownloadProgress = (int)(args.Position.TotalSeconds / args.Duration.TotalSeconds * 100));
        
        StartDownloadingCommand = new RelayCommand(StartDownloading);
        StopDownloadingCommand = new RelayCommand(StopDownloading);
        ClearQueueCommand = new RelayCommand(ClearQueue);
        QueueAllMyTracksCommand = new AsyncCommand(QueueAllMyTracks);
        QueueAllMyPlaylistsCommand = new AsyncCommand(QueueAllMyPlaylists);
        OpenMusicFolderCommand = new AsyncCommand(OpenMusicFolder);
    }

    public async ValueTask AddPlaylistToQueueAsync(IEnumerable<PlaylistTrack> tracks, string title)
    {
        tracks = MapTracks(tracks, title);
        
        if (Application.Current.Dispatcher.CheckAccess())
            DownloadQueue.AddRange(tracks, NotifyCollectionChangedAction.Reset);
        else
            await Application.Current.Dispatcher.InvokeAsync(() => DownloadQueue.AddRange(tracks));
        _snackbarService.Show("Скачивание начато", $"{title} добавлен в очередь");
    }

    private static IEnumerable<PlaylistTrack> MapTracks(IEnumerable<PlaylistTrack> tracks, string title)
    {
        return tracks.Select(b => b with { Data = new DownloaderData(b.Data.Url, 
            b.Data.IsLiked, 
            b.Data.IsExplicit, 
            b.Data.Duration,
            title)});
    }

    public async Task AddPlaylistToQueueAsync(long playlistId, long ownerId, string accessKey)
    {
        var playlist = await vkService.LoadFullPlaylistAsync(playlistId, ownerId, accessKey);
        await AddPlaylistToQueueAsync(playlist.Audios.Select(TrackExtensions.ToTrack), playlist.Playlist.Title!);
    }

    private async Task OpenMusicFolder()
    {
        var config = await configService.GetConfig();

        var directory = config.DownloadDirectory  ??
                       Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "MusicX");
        
        Process.Start(new ProcessStartInfo
        {
            FileName = directory,
            UseShellExecute = true
        });
    }

    private async Task QueueAllMyTracks()
    {
        try
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

        }catch(Exception ex)
        {
            _snackbarService.Show("Ошибка", "Мы не смогли получить все ваши треки :( ");
        }
        
    }

    private async Task QueueAllMyPlaylists()
    {
        var config = await configService.GetConfig();
            
        var playlists = await vkService.GetPlaylistsAsync(config.UserId);

        var allTracks = new List<PlaylistTrack>();
        
        foreach (var playlist in playlists)
        {
            var tracks = await vkService.LoadFullPlaylistAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey);

            allTracks.AddRange(MapTracks(tracks.Items.Select(TrackExtensions.ToTrack), playlist.Title!));
        }
        
        DownloadQueue.AddRange(allTracks, NotifyCollectionChangedAction.Reset);
        
        _snackbarService.Show("Скачивание начато", "Все ваши плейлисты добавлены в очередь");
        
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
            catch (Exception e) when (e is TypeInitializationException or COMException)
            {
                _snackbarService.Show("Упс!", "Кажется ваша система не поддерживает загрузку треков.. Попробуйте обновить Windows до последней версии.");
            }
            catch (OperationCanceledException)
            {
                _snackbarService.Show("Скачивание прервано", "Скачивание очереди было прервано");
            }
            catch (Exception e)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(e, properties);

                logger.Error(e);
                _snackbarService.Show("Ошибка загрузки", "Мы не смогли загрузить трек");
            }
        }

        CurrentDownloadingAudio = null;
        DownloadProgress = 0;
        IsDownloading = false;
    }
}
