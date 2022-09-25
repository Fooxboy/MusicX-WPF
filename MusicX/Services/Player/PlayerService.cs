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

namespace MusicX.Services.Player;

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

    public event EventHandler? NextTrackChanged;
    public event EventHandler? PlayStateChangedEvent;
    public event EventHandler<TimeSpan>? PositionTrackChangedEvent;
    public event EventHandler? TrackChangedEvent;
    public event EventHandler? CurrentPlaylistChanged;

    public event EventHandler<QueueLoadingEventArgs>? QueueLoadingStateChanged;

    private readonly Logger logger;
    private readonly NotificationsService notificationsService;
    private readonly IEnumerable<ITrackMediaSource> _mediaSources;
    private readonly IEnumerable<ITrackStatsListener> _statsListeners;

    private PlaylistTrack? _nextPlayTrack;

    public IPlaylist? CurrentPlaylist { get; set; }

    public PlayerService(Logger logger, NotificationsService notificationsService,
                         IEnumerable<ITrackMediaSource> mediaSources, IEnumerable<ITrackStatsListener> statsListeners)
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
    }

    public async void Play()
    {
        if (CurrentTrack is null) return;
        
        await PlayTrackAsync(CurrentTrack);
        await Task.WhenAll(
            _statsListeners.Select(b => b.TrackPlayStateChangedAsync(CurrentTrack!, player.Position, false)));
    }

    public async Task PlayTrackFromQueueAsync(int index)
    {
        var previousTrack = CurrentTrack!;
        await PlayTrackAsync(Tracks[index]);
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
            TrackChangedEvent.Invoke(this, EventArgs.Empty);
            NextTrackChanged?.Invoke(this, EventArgs.Empty);
        });
        
        var sources = await Task.WhenAll(_mediaSources.Select(b => b.CreateMediaSourceAsync(track)));

        player.Source = sources.First(b => b is not null);
        player.Play();
        UpdateWindowsData().SafeFireAndForget();
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
                await Task.WhenAll(
                    _statsListeners.Select(b => b.TrackChangedAsync(CurrentTrack, firstTrack, ChangeReason.PlaylistChange)));
            }

            QueueLoadingStateChanged?.Invoke(this, new(QueueLoadingState.Started));

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
            QueueLoadingStateChanged.Invoke(this, new(QueueLoadingState.Finished));
        }
    }

    /*public async Task PlayTrack(Audio track, bool loadParentToQueue = true)
    {
        try
        {
            if (CurrentTrack?.Id == track.Id)
            {
                if(IsPlaying) this.Pause();
                else this.Play();
                return;
            }
            var list = new List<object>();

            var startPlayModel = new PlayTrackEvent()
            {
                Event = "music_start_playback",
                AudioId = track.OwnerId + "_" +track.Id,
                Uuid = Guid.NewGuid().GetHashCode(),
                StartTime = "0",
                Shuffle = "false",
                Reason = "auto",
                PlaybackStartedAt = "0",
                TrackCode = track.TrackCode,
                Repeat = "all",
                State = "app",
                Source = track.ParentBlockId,
            };


            if(CurrentTrack != null)
            {
                startPlayModel.PrevAudioId = CurrentTrack.OwnerId + "_" + CurrentTrack.Id;
                var stopTrackModel = new StopTrackEvent()
                {
                    Event = "music_stop_playback",
                    Uuid = Guid.NewGuid().GetHashCode(),
                    Shuffle = "false",
                    Reason = "new",

                    AudioId = CurrentTrack.OwnerId + "_" + CurrentTrack.Id,
                    StartTime = "0",
                    PlaybackStartedAt = CurrentTrack.Duration.ToString(),
                    TrackCode = CurrentTrack.TrackCode,
                    StreamingType = "online",
                    Duration = CurrentTrack.Duration.ToString(),
                    Repeat = "all",
                    State = "app",
                    Source = CurrentTrack.ParentBlockId,

                };

                list.Add(stopTrackModel);
            }

            list.Add(startPlayModel);
            logger.Info($"play track {track.Id}");
               
            CurrentTrack = track;
               
            player.PlaybackSession.Position = TimeSpan.Zero;

            player.Pause();

            var result = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(CurrentTrack.Url));
            var ams = result.MediaSource;

            player.Source = MediaSource.CreateFromAdaptiveMediaSource(ams);

            player.Play();


            new Thread(UpdateWindowsData).Start();
            //player.SystemMediaTransportControls.DisplayUpdater.ClearAll();


            string artist;


            config = await configService.GetConfig();

            if (config.ShowRPC == null)
            {
                config.ShowRPC = true;

                await configService.SetConfig(config);
            }

            if(config.BroadcastVK == null)
            {
                config.BroadcastVK = false;

                await configService.SetConfig(config);
            }


            if(config.BroadcastVK.Value)
            {
                await vkService.SetBroadcastAsync(track);
            }

            if (config.ShowRPC.Value)
            {
                if (CurrentTrack.MainArtists?.Count > 0)
                {
                    string s = string.Empty;
                    foreach (var trackArtist in CurrentTrack.MainArtists)
                    {
                        s += trackArtist.Name + ", ";
                    }

                    var artists = s.Remove(s.Length - 2);

                    artist = artists;

                }
                else
                {
                    artist = CurrentTrack.Artist;
                }

                TimeSpan t = TimeSpan.FromSeconds(CurrentTrack.Duration);

                string cover = "";
                if(track.Album == null)
                {
                    cover = "album";
                }else
                {
                    cover = track.Album.Cover;
                }

                discordService.RemoveTrackPlay();
                discordService.SetTrackPlay(artist, CurrentTrack.Title, t, cover);
            }


            TrackChangedEvent?.Invoke(this, EventArgs.Empty);

            PositionTrackChangedEvent?.Invoke(this, TimeSpan.Zero);

            logger.Info($"played track {track.Id}");

            await vkService.StatsTrackEvents(list);

            if (track.ParentBlockId != null && loadParentToQueue)
            {
                Debug.WriteLine("track.ParentBlockId != null");
                CurrentPlaylist = null;

                if (track.ParentBlockId == this.CurrentBlockId)
                {
                    CurrentIndex = Tracks.IndexOf(Tracks.Single(a => a.Id == CurrentTrack.Id));


                    if (CurrentIndex + 1 > Tracks.Count - 1)
                    {
                        NextPlayTrack = CurrentTrack;
                    }
                    else
                    {
                        NextPlayTrack = Tracks[CurrentIndex + 1];

                    }
                    return;
                }

                Debug.WriteLine($"track.ParentBlockId = {track.ParentBlockId} | this.blockId = {this.CurrentBlockId} ");

                Debug.WriteLine("track.ParentBlockId != this.blockId");

                await Task.Run(async () =>
                {
                    try
                    {
                        Debug.WriteLine($"LOAD TRACKS BY BLOCK");

                        logger.Info("Get current track block info");
                        this.CurrentBlockId = track.ParentBlockId;
                        CurrentPlaylistId = -1;
                        var items = await vkService.LoadFullAudiosAsync(CurrentBlockId).ToListAsync();

                        await Application.Current.Dispatcher.InvokeAsync(() => Tracks.ReplaceRange(items));

                        int c = 0;
                        foreach (var trackDebug in Tracks)
                        {
                            Debug.WriteLine($"[{c}]{trackDebug.Artist} - {trackDebug.Title}");
                            c++;
                        }

                        CurrentIndex = Tracks.IndexOf(Tracks.Single(a => a.Id == track.Id));

                        Debug.WriteLine($"Played track with index: {CurrentIndex}");

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in player service, playTrack => get block items");
                        logger.Error(ex, ex.Message);

                        notificationsService.Show("Ошибка", "Мы не смогли загрузить очередь воспроизведения");
                    }

                });
            }
            else
            {
                Debug.WriteLine("LOAD FROM PLAYLIST");

                if (CurrentPlaylistId == CurrentPlaylist?.PlaylistId)
                {
                    CurrentIndex = Tracks.IndexOf(Tracks.Single(a => a.Id == track.Id));

                    if (CurrentIndex + 1 > Tracks.Count - 1)
                    {
                        NextPlayTrack = CurrentTrack;
                    }
                    else
                    {
                        NextPlayTrack = Tracks[CurrentIndex + 1];

                    }
                    return;
                }

                if (CurrentPlaylist is null)
                    return;

                var (playlistId, ownerId, accessKey) = CurrentPlaylist;

                Debug.WriteLine($"loadedPlaylistIdTracks = {CurrentPlaylistId} |  plViewModel.Playlist.Id = {playlistId}");

                logger.Info($"Load tracks with playlist id {playlistId}");

                var fullPlaylist = await vkService.LoadFullPlaylistAsync(playlistId, ownerId, accessKey);
                await Application.Current.Dispatcher.InvokeAsync(() => Tracks.ReplaceRange(fullPlaylist.Audios));

                Debug.WriteLine("Now play queue:");

                int c = 0;
                foreach (var trackDebug in Tracks)
                {
                    Debug.WriteLine($"[{c}]{trackDebug.Artist} - {trackDebug.Title}");
                    c++;
                }

                CurrentPlaylistId = playlistId;
                CurrentIndex = Tracks.IndexOf(Tracks.Single(a => a.Id == track.Id));

                Debug.WriteLine($"Played track with index: {CurrentIndex}");

                if (CurrentIndex + 1 > Tracks.Count - 1)
                {
                    NextPlayTrack = CurrentTrack;
                }
                else
                {
                    NextPlayTrack = Tracks[CurrentIndex + 1];

                }
                return;
            }

            CurrentIndex = Tracks.IndexOf(Tracks.Single(a => a.Id == CurrentTrack.Id));


            if (CurrentIndex + 1 > Tracks.Count - 1)
            {
                NextPlayTrack = CurrentTrack;
            }
            else
            {
                NextPlayTrack = Tracks[CurrentIndex + 1];

            }
        }
        catch (Exception ex)
        {
            logger.Error("Error in playerService in PlayTrack:");
            logger.Error(ex, ex.Message);

            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");

        }
        finally
        {
            await Application.Current.Dispatcher.InvokeAsync(() => QueueLoadingStateChanged.Invoke(this, new(QueueLoadingState.Finished)));
        }
    }

    public async Task PlayBoomTrack(Track track, List<Track> tracks)
    {
        try
        {
            if(boomClient == null)
            {
                var config = await configService.GetConfig();
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "okhttp/5.0.0-alpha.10");
                client.DefaultRequestHeaders.Add("X-App-Id", "6767438");
                client.DefaultRequestHeaders.Add("X-Client-Version", "10265");

                client.DefaultRequestHeaders.Authorization = new ("Bearer", config.BoomToken);

                boomClient = client;

            }
            player.PlaybackSession.Position = TimeSpan.Zero;
            player.Pause();


            //var result = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(track.File), boomClient);

            //var mediaSource = result.MediaSource;
            //var source = MediaSource.CreateFromAdaptiveMediaSource(mediaSource);
            //player.Source = source;

            var response = await boomClient.GetAsync(track.File);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            player.Source = MediaSource.CreateFromStream(stream.AsRandomAccessStream(),
                                                         response.Content.Headers.ContentType?.MediaType ??
                                                         "audio/mpeg");
            player.Play();

            //new Thread(UpdateWindowsData).Start();
        }
        catch (Exception ex)
        {

        }
    }*/

    /*public async Task Play(int index, List<Audio> tracks = null)
    {
        try
        {
            logger.Info($"Play track with index = {index}");
            Audio track = null;

            if (tracks != null) track = tracks[index];
            else track = Tracks[index];


            var list = new List<object>();

            var startPlayModel = new PlayTrackEvent()
            {
                Event = "music_start_playback",
                AudioId = track.OwnerId + "_" + track.Id,
                Uuid = Guid.NewGuid().GetHashCode(),
                StartTime = "0",
                Shuffle = "false",
                Reason = "auto",
                PlaybackStartedAt = "0",
                TrackCode = track.TrackCode,
                Repeat = "all",
                State = "app",
                Source = track.ParentBlockId,
            };


            if (CurrentTrack != null)
            {
                startPlayModel.PrevAudioId = CurrentTrack.OwnerId + "_" + CurrentTrack.Id;
                var stopTrackModel = new StopTrackEvent()
                {
                    Event = "music_stop_playback",
                    Uuid = Guid.NewGuid().GetHashCode(),
                    Shuffle = "false",
                    Reason = "new",
                    AudioId = CurrentTrack.OwnerId + "_" + CurrentTrack.Id,
                    StartTime = "0",
                    PlaybackStartedAt = CurrentTrack.Duration.ToString(),
                    TrackCode = CurrentTrack.TrackCode,
                    StreamingType = "online",
                    Duration = Position.TotalSeconds.ToString(),
                    Repeat = "all",
                    State = "app",
                    Source = CurrentTrack.ParentBlockId,

                };

                list.Add(stopTrackModel);
            }

            list.Add(startPlayModel);

            logger.Info($"play track with index = {index}");
            CurrentIndex = index;
            player.PlaybackSession.Position = TimeSpan.Zero;

            player.Pause();
            if (tracks != null)
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                    await Application.Current.Dispatcher.InvokeAsync(() => Tracks.ReplaceRange(tracks));
                else
                    Tracks.ReplaceRange(tracks);

                int c = 0;
                foreach (var trackDebug in Tracks)
                {
                    Debug.WriteLine($"[{c}]{trackDebug.Artist} - {trackDebug.Title}");
                    c++;
                }

                Debug.WriteLine($"Now play track with index: {index}");

            }
            Debug.WriteLine($"Now play track with index: {index}");

            CurrentTrack = Tracks[index];

            if (CurrentIndex + 1 > Tracks.Count - 1)
            {
                NextPlayTrack = CurrentTrack;
            }
            else
            {
                NextPlayTrack = Tracks[CurrentIndex + 1];

            }

            if (string.IsNullOrEmpty(CurrentTrack.Url))
            {
                await NextTrack();
                logger.Info("Track url in empty. Next track...");
                return;
            }

            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                TrackChangedEvent?.Invoke(this, EventArgs.Empty);

            });

            AdaptiveMediaSourceCreationResult result = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(CurrentTrack.Url));
            var ams = result.MediaSource;

            player.Source = MediaSource.CreateFromAdaptiveMediaSource(ams);

                
            player.Play();


            await vkService.SetBroadcastAsync(track);


            new Thread(UpdateWindowsData).Start();


            config = await configService.GetConfig();

            if (config.ShowRPC == null)
            {
                config.ShowRPC = true;

                await configService.SetConfig(config);
            }

            if (config.BroadcastVK == null)
            {
                config.BroadcastVK = false;

                await configService.SetConfig(config);
            }


            if (config.BroadcastVK.Value)
            {
                await vkService.SetBroadcastAsync(track);
            }


            if (config.ShowRPC.Value)
            {
                await SetDiscordTrack();

            }

            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                PositionTrackChangedEvent?.Invoke(this, TimeSpan.Zero);

            });

            await vkService.StatsTrackEvents(list);


        }
        catch (Exception ex)
        {
            logger.Error("Error in playerService, Play(index)");
            logger.Error(ex, ex.Message);

            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");

        }

    }*/
    
    /*public async Task TryPlay()
    {
        try
        {
            logger.Info("Try play track");
            player.PlaybackSession.Position = TimeSpan.Zero;
            player.Pause();

            AdaptiveMediaSourceCreationResult result = await AdaptiveMediaSource.CreateFromUriAsync(new Uri(CurrentTrack.Url));
            var ams = result.MediaSource;
            player.Source = MediaSource.CreateFromAdaptiveMediaSource(ams);
            player.Play();
            new Thread(UpdateWindowsData).Start();

            PositionTrackChangedEvent?.Invoke(this, TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            logger.Error("Error in try play track");
            logger.Error(ex, ex.Message);
            await NextTrack();

            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");

        }

    }*/
    
    /*public async void Play()
    {
        try
        {
            if (CurrentTrack is null) return;
            player.Play();

            if(config.ShowRPC.Value)
            {
                await SetDiscordTrack();
            }
        }catch(Exception ex)
        {
            logger.Error("Error in play");
            logger.Error(ex, ex.Message);

            notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");

        }

    }*/

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

    public void Seek(TimeSpan position)
    {
        try
        {
            player.PlaybackSession.Position = position;

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

    public void SetRepeat(bool repeat)
    {
        logger.Info($"SET REPEAT: {repeat}");

        this.IsRepeat = repeat;
    }


    private async void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {



        //if (args.Error == MediaPlayerError.SourceNotSupported)
        //{

        //}

        //Ошибка при загрузке

        if (args.Error == MediaPlayerError.SourceNotSupported)
        {
            logger.Error("Error SourceNotSupported player");

            notificationsService.Show("Ошибка", "Произошла ошибкба SourceNotSupported");


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