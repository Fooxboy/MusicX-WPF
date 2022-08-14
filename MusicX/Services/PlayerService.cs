using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Models;
using MusicX.ViewModels;
using NLog;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage.Streams;
using MusicX.Helpers;

namespace MusicX.Services
{
    public enum QueueLoadingState
    {
        Started,
        Finished
    }
    public record QueueLoadingEventArgs(QueueLoadingState State);
    public class PlayerService
    {
        public int CurrentIndex;
        public string CurrentBlockId { get; set; }
        public long CurrentPlaylistId
        {
            get => _currentPlaylistId;
            set
            {
                _currentPlaylistId = value;
                Application.Current.Dispatcher.BeginInvoke(() => CurrentPlaylistChanged?.Invoke(this, EventArgs.Empty));
            }
        }
        public readonly ObservableRangeCollection<Audio> Tracks = new();
        public Audio CurrentTrack;
        public Audio NextPlayTrack
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
        public event EventHandler PlayStateChangedEvent;
        public event EventHandler<TimeSpan> PositionTrackChangedEvent;
        public event EventHandler TrackChangedEvent;
        public event EventHandler? CurrentPlaylistChanged;

        public event EventHandler<QueueLoadingEventArgs> QueueLoadingStateChanged; 

        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly DiscordService discordService;
        private readonly ConfigService configService;
        private readonly NotificationsService notificationsService;

        private ConfigModel config;
        private Audio _nextPlayTrack;
        private long _currentPlaylistId;
        
        public PlaylistData? CurrentPlaylist { get; set; }

        public PlayerService(VkService vkService, Logger logger, DiscordService discordService, ConfigService configService, NotificationsService notificationsService)
        {
            this.vkService = vkService;
            this.logger = logger;
            this.discordService = discordService;
            this.configService = configService;
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
            this.discordService = discordService;
            this.notificationsService = notificationsService;

           
        }

        public async Task PlayTrack(Audio track, bool loadParentToQueue = true)
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


        private void UpdateWindowsData()
        {
            try
            {
                Thread.Sleep(1000);

                var updater = player.SystemMediaTransportControls.DisplayUpdater;


                updater.MusicProperties.Title = CurrentTrack.Title;
                updater.MusicProperties.Artist = CurrentTrack.Artist;
                updater.MusicProperties.TrackNumber = 1;
                updater.MusicProperties.AlbumArtist = CurrentTrack.Artist;
                updater.MusicProperties.AlbumTitle = CurrentTrack.Title;
                updater.MusicProperties.AlbumTrackCount = 1;



                if (CurrentTrack.Album != null)
                {
                    player.SystemMediaTransportControls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(CurrentTrack.Album.Cover));

                }

                player.SystemMediaTransportControls.DisplayUpdater.Update();
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
           
        }

        public async Task Play(int index, List<Audio> tracks = null)
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

        }

        private async Task SetDiscordTrack()
        {
            string artist;

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

            t -= player.Position;


            string cover = "";
            if (CurrentTrack.Album == null)
            {
                cover = "album";
            }
            else
            {
                cover = CurrentTrack.Album.Cover;
            }

            discordService.RemoveTrackPlay();

            discordService.SetTrackPlay(artist, CurrentTrack.Title, t, cover);
        }

        public async Task TryPlay()
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

        }

        public async Task NextTrack()
        {
            try
            {
                logger.Info("Next track");
                
                if (CurrentIndex + 1 > Tracks.Count - 1)
                {
                    return;
                }

                CurrentTrack = Tracks[CurrentIndex + 1];
                CurrentIndex += 1;
                
                await Play(CurrentIndex, null);
            }catch(Exception ex)
            {
                logger.Error("Error in playerService => NextTrack");
                logger.Error(ex, ex.Message);

                notificationsService.Show("Ошибка", "Произошла ошибка при воспроизведении");

            }
        }

        public async void Play()
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
      

        public bool IsPlaying => player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing
             || player.PlaybackSession.PlaybackState == MediaPlaybackState.Opening
             || player.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering;

        

        public void SetTracks(List<Audio> tracks)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
                Application.Current.Dispatcher.Invoke(() => Tracks.ReplaceRange(tracks));
            else
                Tracks.ReplaceRange(tracks);
        }

        public async void Pause()
        {

            if (config == null)
            {
                config = await configService.GetConfig();
            }

            if (config.ShowRPC == null)
            {
                config.ShowRPC = true;

                await configService.SetConfig(config);
            }

            if (config.ShowRPC.Value)
            {
                discordService.RemoveTrackPlay(true);
            }

            try
            {
                player.Pause();

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

                    TrackChangedEvent?.Invoke(this, EventArgs.Empty);
                    await Play(index, null);
                }
            }
            catch (Exception e)
            {
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
                await PlayTrack(Tracks[0], false).ConfigureAwait(false);
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


                //audio source url may expire
                await TryPlay();
                return;
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
        public async void RemoveFromQueue(Audio audio)
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
            
            CurrentIndex = Tracks.IndexOf(CurrentTrack);
            
            if (audio == NextPlayTrack)
                NextPlayTrack = Tracks.ElementAtOrDefault(CurrentIndex + 1);
        }

        public async void InsertToQueue(Audio audio, bool afterCurrent)
        {
            if (Tracks.Count == 0 && !IsPlaying)
            {
                Tracks.Add(audio);
                // so we dont had to deal with deadlocks if insertion was triggered with dispatcher context
                await PlayTrack(audio, false).ConfigureAwait(false);
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
}
