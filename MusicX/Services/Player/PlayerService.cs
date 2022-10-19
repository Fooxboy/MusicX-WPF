using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage.Streams;
using AsyncAwaitBestPractices;
using MusicX.Core.Models;
using MusicX.Core.Models.Boom;
using MusicX.Helpers;
using MusicX.Models;
using MusicX.Services.Player.Playlists;
using MusicX.Services.Player.TrackStats;
using MusicX.ViewModels;
using NLog;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Shared.Player;

namespace MusicX.Services.Player;

public enum PlayerMode
{
    None,
    Master,
    Slave
}

public class PlayerService
{
    public int CurrentIndex;
    
    public readonly ObservableRangeCollection<PlaylistTrack> Tracks = new();
    public PlaylistTrack? CurrentTrack { get; private set; }
    public PlaylistTrack? NextPlayTrack
    {
        get => _nextPlayTrack;
        set
        {
            _nextPlayTrack = value;
            Application.Current.Dispatcher.BeginInvoke(() => NextTrackChanged?.Invoke(this, EventArgs.Empty));
        }
    }
    private DispatcherTimer _positionTimer;
    private readonly MediaPlayer player;

    public bool IsShuffle { get; set; }
    public bool IsRepeat { get; set; }

    public PlayerMode Mode { get; private set; } = PlayerMode.None;
    public string? SlaveSessionId {get; private set; }

    public event EventHandler? NextTrackChanged;
    public event EventHandler? PlayStateChangedEvent;
    public event EventHandler<TimeSpan>? PositionTrackChangedEvent;
    public event EventHandler? TrackChangedEvent;
    public event EventHandler? CurrentPlaylistChanged;

    public event EventHandler<PlayerLoadingEventArgs>? QueueLoadingStateChanged;
    public event EventHandler<PlayerLoadingEventArgs>? TrackLoadingStateChanged;

    private readonly Logger logger;
    private readonly NotificationsService notificationsService;
    private readonly IEnumerable<ITrackMediaSource> _mediaSources;
    private readonly IEnumerable<ITrackStatsListener> _statsListeners;
    private readonly ServerService _serverService;

    private PlaylistTrack? _nextPlayTrack;

    public IPlaylist? CurrentPlaylist { get; set; }

    public PlayerService(Logger logger, NotificationsService notificationsService,
                         IEnumerable<ITrackMediaSource> mediaSources, IEnumerable<ITrackStatsListener> statsListeners, ServerService serverService)
    {
        this.logger = logger;
        player = new MediaPlayer();

        player.AudioCategory = MediaPlayerAudioCategory.Media;
        player.Play();


        try
        {
            player.PlaybackSession.PlaybackStateChanged += MediaPlayerOnCurrentStateChanged;
            player.MediaEnded += MediaPlayerOnMediaEnded;
            player.MediaFailed += MediaPlayerOnMediaFailed;

            player.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            player.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            //player.CommandManager.ShuffleBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            //player.CommandManager.AutoRepeatModeBehavior.EnablingRule = MediaCommandEnablingRule.Always;

            player.SystemMediaTransportControls.DisplayUpdater.Type = Windows.Media.MediaPlaybackType.Music;


            player.CommandManager.NextReceived += async (c, e) => await NextTrack();
            player.CommandManager.PreviousReceived += async (c, e) => await PreviousTrack();
            player.CommandManager.PlayReceived += (c, e) => Play();
            player.CommandManager.PauseReceived += (c, e) => Pause();
        }catch(Exception ex)
        {
            logger.Error(ex, ex.Message);
        }
          

        _positionTimer = new DispatcherTimer();
        _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
        _positionTimer.Tick += PositionTimerOnTick;
        this.notificationsService = notificationsService;
        _mediaSources = mediaSources;
        _statsListeners = statsListeners;
        _serverService = serverService;
        
        _serverService.PlayStateChanged += ServerOnPlayStateChanged;
    }

    private Task ServerOnPlayStateChanged(TimeSpan position, bool pause)
    {
        if (Mode is not PlayerMode.Slave)
            return Task.CompletedTask;

        if (Math.Abs(position.TotalSeconds - Position.TotalSeconds) > 2)
            Seek(position);

        if (pause == !IsPlaying) return Task.CompletedTask;
        
        if (pause)
            Pause();
        else
            Play();

        return Task.CompletedTask;
    }

    public async Task PlayFromAsync(long userId)
    {
        switch (Mode)
        {
            case PlayerMode.Master:
                await _serverService.StopSessionAsync();
                break;
            case PlayerMode.Slave when SlaveSessionId is not null:
                await _serverService.LeaveSessionAsync(SlaveSessionId);
                break;
        }

        Mode = PlayerMode.Slave;
        SlaveSessionId = userId.ToString();
        
        var playlist = await _serverService.JoinSessionAsync(SlaveSessionId);

        await PlayAsync(playlist);
    }

    public async void Play()
    {
        if (CurrentTrack is null) return;
        
        await PlayTrackAsync(CurrentTrack);
        
        if (Application.Current.Dispatcher.CheckAccess())
            PlayStateChangedEvent?.Invoke(this, EventArgs.Empty);
        else
            await Application.Current.Dispatcher.InvokeAsync(
                () => PlayStateChangedEvent?.Invoke(this, EventArgs.Empty));
        
        if (Mode is PlayerMode.Master)
            await Task.WhenAll(
                _statsListeners.Select(b => b.TrackPlayStateChangedAsync(CurrentTrack!, player.Position, false)));
    }

    public async Task PlayTrackFromQueueAsync(int index)
    {
        var previousTrack = CurrentTrack!;
        await PlayTrackAsync(Tracks[index]);
        if (Mode is PlayerMode.Master)
            await Task.WhenAll(
                _statsListeners.Select(b => b.TrackChangedAsync(previousTrack, CurrentTrack!, ChangeReason.TrackChange)));
    }
    
    private async Task PlayTrackAsync(PlaylistTrack track)
    {
        if (CurrentTrack == track)
        {
            player.Play();
            return;
        }
        
        player.PlaybackSession.Position = TimeSpan.Zero;
        player.Pause();
        
        CurrentTrack = track;
        CurrentIndex = Tracks.IndexOf(track);
        NextPlayTrack = Tracks.ElementAtOrDefault(CurrentIndex + 1);

        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            TrackChangedEvent?.Invoke(this, EventArgs.Empty);
            NextTrackChanged?.Invoke(this, EventArgs.Empty);
            PlayStateChangedEvent?.Invoke(this, EventArgs.Empty);
            TrackLoadingStateChanged?.Invoke(this, new(PlayerLoadingState.Started));
        });

        if (CurrentTrack.Data.Url is null) await NextTrack();

        if (Mode is PlayerMode.Slave && SlaveSessionId is not null)
            await _serverService.LeaveSessionAsync(SlaveSessionId);

        if (Mode is not PlayerMode.Master)
        {
            Mode = PlayerMode.Master;
            await _serverService.StartSessionAsync();
        }

        var sources = new List<MediaSource?>();
        foreach(var source in _mediaSources)
        {
            var item = await source.CreateMediaSourceAsync(track);
            sources.Add(item);
        }

        if (sources.All(m => m is null)) await NextTrack();
        player.Source = sources.First(b => b is not null);
        player.Play();
        UpdateWindowsData().SafeFireAndForget();
        
        Application.Current.Dispatcher.BeginInvoke(
            () => TrackLoadingStateChanged?.Invoke(this, new(PlayerLoadingState.Finished)));
    }

    public async Task PlayAsync(IPlaylist playlist, PlaylistTrack? firstTrack = null)
    {
        if (!playlist.CanLoad)
            throw new InvalidOperationException("Playlist should be loadable for first play");

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            await Application.Current.Dispatcher.InvokeAsync(() => PlayAsync(playlist).SafeFireAndForget());
        }

        try
        {
            CurrentPlaylist = playlist;
            CurrentPlaylistChanged?.Invoke(this, EventArgs.Empty);

            Task? firstTrackTask = null;
            if (firstTrack is not null)
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    Tracks.Clear();
                    Tracks.Add(firstTrack);
                }
                else
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Tracks.Clear();
                        Tracks.Add(firstTrack);
                    });
                }
                
                firstTrackTask = PlayTrackAsync(firstTrack);
                if (Mode is PlayerMode.Master)
                    await Task.WhenAll(
                        _statsListeners.Select(b => b.TrackChangedAsync(CurrentTrack, firstTrack, ChangeReason.PlaylistChange)));
            }

            QueueLoadingStateChanged?.Invoke(this, new(PlayerLoadingState.Started));

            var loadTask = playlist.LoadAsync().ToArrayAsync().AsTask();

            if (firstTrackTask is null)
                await loadTask;
            else
                await Task.WhenAll(loadTask, firstTrackTask);
            
            Tracks.ReplaceRange(loadTask.Result);
            CurrentIndex = Tracks.IndexOf(CurrentTrack!);

            if (!IsPlaying)
            {
                var previousTrack = CurrentTrack;
                await PlayTrackAsync(Tracks[0]);
                if (Mode is PlayerMode.Master)
                    await Task.WhenAll(
                        _statsListeners.Select(b => b.TrackChangedAsync(previousTrack, CurrentTrack!, ChangeReason.PlaylistChange)));
            }
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
            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");
        }
        finally
        {
            QueueLoadingStateChanged.Invoke(this, new(PlayerLoadingState.Finished));
        }
    }

    private async Task UpdateWindowsData()
    {
        try
        {
            await Task.Delay(1000);

            var updater = player.SystemMediaTransportControls.DisplayUpdater;


            updater.MusicProperties.Title = CurrentTrack.Title;
            updater.MusicProperties.Artist = CurrentTrack.GetArtistsString();
            updater.MusicProperties.TrackNumber = 1;
            updater.MusicProperties.AlbumArtist = CurrentTrack.MainArtists.First().Name;
            updater.MusicProperties.AlbumTitle = CurrentTrack.AlbumId?.Name ?? string.Empty;
            updater.MusicProperties.AlbumTrackCount = 1;



            if (CurrentTrack.AlbumId is not null)
            {
                player.SystemMediaTransportControls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(CurrentTrack.AlbumId.CoverUrl));

            }

            player.SystemMediaTransportControls.DisplayUpdater.Update();
        }catch(Exception ex)
        {

            var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
            Crashes.TrackError(ex, properties);

            logger.Error(ex, ex.Message);
        }
           
    }

    public async Task NextTrack()
    {
        try
        {
            logger.Info("Next track");

            async Task LoadMore()
            {
                var array = await CurrentPlaylist!.LoadAsync().ToArrayAsync();
                if (Application.Current.Dispatcher.CheckAccess())
                    Tracks.AddRangeSequential(array);
                else
                    await Application.Current.Dispatcher.InvokeAsync(() => Tracks.AddRangeSequential(array));
            }
            
            if (CurrentIndex + 1 > Tracks.Count - 1)
            {
                if (CurrentPlaylist?.CanLoad == false)
                    return;
                await LoadMore();
            }

            var nextTrack = Tracks[CurrentIndex + 1];
            CurrentIndex += 1;
            
            // its last track and we can load more
            if (CurrentIndex == Tracks.Count - 1 && CurrentPlaylist?.CanLoad == true)
            {
                await LoadMore();
            }

            var previousTrack = CurrentTrack!;

            await PlayTrackAsync(nextTrack);
            if (Mode is PlayerMode.Master)
                await Task.WhenAll(
                    _statsListeners.Select(b => b.TrackChangedAsync(previousTrack, nextTrack, ChangeReason.NextButton)));
        }catch(Exception ex)
        {
            var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
            Crashes.TrackError(ex, properties);
            logger.Error("Error in playerService => NextTrack");
            logger.Error(ex, ex.Message);

            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");

        }
    }

    public double Volume
    {
        get => player.Volume;
        set
        {
            if (player.Volume == value)
                return;

            player.Volume = value;
        }
    }

    public bool IsMuted
    {
        get => player.IsMuted; 
        set => player.IsMuted = value;
    }

      
    public TimeSpan Position => player.PlaybackSession.Position;
    public TimeSpan Duration => player?.NaturalDuration ?? TimeSpan.Zero;
      

    public bool IsPlaying => player.PlaybackSession.PlaybackState is MediaPlaybackState.Playing or MediaPlaybackState.Opening or MediaPlaybackState.Buffering;

    public async void Pause()
    {
        try
        {
            player.Pause();
            
            if (Application.Current.Dispatcher.CheckAccess())
                PlayStateChangedEvent?.Invoke(this, EventArgs.Empty);
            else
                await Application.Current.Dispatcher.InvokeAsync(
                    () => PlayStateChangedEvent?.Invoke(this, EventArgs.Empty));
            
            if (Mode is PlayerMode.Master)
                await Task.WhenAll(
                    _statsListeners.Select(b => b.TrackPlayStateChangedAsync(CurrentTrack!, player.Position, true)));
        }catch (Exception ex)
        {
            logger.Error(ex, ex.Message);
        }
    }

    public void SetVolume(double volume)
    {
        Volume = volume;
    }

    public async void Seek(TimeSpan position)
    {
        try
        {
            player.PlaybackSession.Position = position;

            if (Mode is PlayerMode.Master)
                await Task.WhenAll(
                    _statsListeners.Select(b => b.TrackPlayStateChangedAsync(CurrentTrack!, position, !IsPlaying)));
        }
        catch (Exception e)
        {
            logger.Error(e, e.Message);
        }
    }
    
    public async Task PreviousTrack()
    {
        try
        {
            logger.Info("Go to prev track");
            if (Position.TotalSeconds > 10) Seek(TimeSpan.Zero);
            else
            {

                var index = CurrentIndex - 1;
                if (index < 0) index = Tracks.Count - 1;

                var previousTrack = Tracks[index];
                var prevCurrentTrack = CurrentTrack!;
                
                await PlayTrackAsync(previousTrack);
                
                if (Mode is PlayerMode.Master)
                    await Task.WhenAll(
                        _statsListeners.Select(b => b.TrackChangedAsync(prevCurrentTrack, previousTrack!, ChangeReason.PrevButton)));
            }
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

            logger.Error("Error in playerService => PreviousTrack");
            logger.Error(e, e.Message);

            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");


        }

    }

    private void MediaPlayerOnCurrentStateChanged(MediaPlaybackSession sender, object args)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (sender.PlaybackState == MediaPlaybackState.Playing)
                    _positionTimer.Start();
                else
                    _positionTimer.Stop();

                PlayStateChangedEvent?.Invoke(this, EventArgs.Empty);
            });
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

            logger.Error(e, e.Message);

            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");

        }

    }

    private async void MediaPlayerOnMediaEnded(MediaPlayer sender, object args)
    {
        try
        {
            if(IsRepeat)
            {
                this.player.Pause();
                Seek(TimeSpan.Zero);
                this.player.Play();
                return;
            }
            await NextTrack();
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

            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");

            logger.Error(e, e.Message);
        }
    }


    public async void SetShuffle(bool shuffle)
    {
        try
        {
            logger.Info($"SET SHUFFLE: {shuffle}");
            this.IsShuffle = shuffle;
            if (!shuffle)
                return;

            var list = Tracks.ToList();
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var k = Random.Shared.Next(i + 1);
                (list[k], list[i]) = (list[i], list[k]);
            }
            Tracks.ReplaceRange(list);

            if (CurrentTrack != Tracks[0])
                await PlayTrackAsync(Tracks[0]).ConfigureAwait(false);

        }
        catch(Exception ex)
        {
            var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
            Crashes.TrackError(ex, properties);

            notificationsService.Show("Ошибка", "Произошла ошибка при перемешивании");

            logger.Error(ex, ex.Message);
        }
        
    }

    public void SetRepeat(bool repeat)
    {
        logger.Info($"SET REPEAT: {repeat}");

        this.IsRepeat = repeat;
    }


    private async void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        if (args.Error == MediaPlayerError.SourceNotSupported)
        {
            logger.Error("Error SourceNotSupported player");

            notificationsService.Show("Ошибка", "Произошла ошибкба SourceNotSupported");

            if(CurrentTrack is not null && CurrentTrack.Data.Url.EndsWith(".mp3"))
            {
                try
                {
                    //Какая же это все грязь, пиздец.

                    var vkService = StaticService.Container.GetRequiredService<VkService>();

                    var boomService = StaticService.Container.GetRequiredService<BoomService>();
                    var configService = StaticService.Container.GetRequiredService<ConfigService>();

                    var config = await configService.GetConfig();
                    var boomVkToken = await vkService.GetBoomToken();

                    var boomToken = await boomService.AuthByTokenAsync(boomVkToken.Token, boomVkToken.Uuid);

                    config.BoomToken = boomToken.AccessToken;
                    config.BoomTokenTtl = DateTimeOffset.Now + TimeSpan.FromSeconds(boomToken.ExpiresIn);
                    config.BoomUuid = boomVkToken.Uuid;

                    await configService.SetConfig(config);

                    boomService.SetToken(boomToken.AccessToken);

                    await PlayTrackAsync(CurrentTrack);

                }
                catch (Exception ex)
                {
                    var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                    Crashes.TrackError(ex, properties);

                }

                return;
            }

            if (CurrentTrack is not null)
                //audio source url may expire
                await PlayTrackAsync(CurrentTrack);
        }
        else if (args.Error == MediaPlayerError.NetworkError)
        {
            notificationsService.Show("Ошибка", "Мы не смогли воспроизвести трек из-за проблем с сетью");

            logger.Error("Network Error player");
        }

            


        //DispatcherHelper.CheckBeginInvokeOnUI(async () =>
        //{
        //    await ContentDialogService.Show(new ExceptionDialog("Невозможно загрузить аудио файл", $"Невозможно загрузить файл по этой причине: {args.Error.ToString()}", new Exception(args.ErrorMessage)));

        //});
        //Log.Error("Media failed. " + args.Error + " " + args.ErrorMessage);
    }

    private void PositionTimerOnTick(object sender, object o)
    {
        Application.Current.Dispatcher.Invoke((() =>
        {
            PositionTrackChangedEvent?.Invoke(this, Position);
        }));
    }
    public async void RemoveFromQueue(PlaylistTrack audio)
    {
        if (audio == CurrentTrack)
        {
            if (CurrentIndex + 1 < Tracks.Count)
                await NextTrack();
            else
                Pause();
        }

        if (!Tracks.Remove(audio))
            return;
            
        CurrentIndex = Tracks.IndexOf(CurrentTrack!);
            
        if (audio == NextPlayTrack)
            NextPlayTrack = Tracks.ElementAtOrDefault(CurrentIndex + 1);
    }

    public async void InsertToQueue(PlaylistTrack audio, bool afterCurrent)
    {
        if (Tracks.Count == 0 && !IsPlaying)
        {
            Tracks.Add(audio);
            // so we dont had to deal with deadlocks if insertion was triggered with dispatcher context
            await PlayTrackAsync(audio).ConfigureAwait(false);
            return;
        }
            
        if (afterCurrent)
        {
            Tracks.Insert(CurrentIndex + 1, audio);
            NextPlayTrack = audio;
        }
        else
        {
            // if current is last track in the queue
            if (CurrentIndex == Tracks.Count - 1)
                NextPlayTrack = audio;
                
            Tracks.Add(audio);
        }
            
        await Application.Current.Dispatcher.InvokeAsync(() => NextTrackChanged?.Invoke(this, EventArgs.Empty));
    }
}