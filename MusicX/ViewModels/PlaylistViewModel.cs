using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using NLog;
using Wpf.Ui;

namespace MusicX.ViewModels
{
    [Serializable]
    public record PlaylistData(long PlaylistId, long OwnerId, string AccessKey);
    
    public class PlaylistViewModel:BaseViewModel
    {

        public event EventHandler<Playlist> PlaylistLoaded;

        public event EventHandler<Playlist> PlaylistNotLoaded;
        public string Title { get; set; }
        public string ArtistText { get; set; }
        public string Genres { get; set; }
        public string Year { get; set; }
        public string Plays { get; set; }
        public string Description { get; set; }
        public Visibility VisibleLoading { get; set; } = Visibility.Visible;
        public Visibility VisibleContent { get; set; } = Visibility.Collapsed;
        public string Cover { get; set; }
        public ObservableRangeCollection<Audio> Tracks { get; } = new();

        public Visibility VisibileAddInfo { get; set; } = Visibility.Visible;

        public Playlist Playlist { get; private set; }
        public PlaylistData PlaylistData { get; set; }
        
        public Visibility VisibleLoadingMore { get; set; } = Visibility.Collapsed;

        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly ISnackbarService _snackbarService;

        public ConfigService ConfigService { get; set; }

        public PlaylistViewModel(VkService vkService, Logger logger, ConfigService configService,
            ISnackbarService snackbarService)
        {
            this.vkService = vkService;
            this.ConfigService = configService;
            this.logger = logger;

            _snackbarService = snackbarService;
        }
        public async ValueTask LoadMore()
        {
            try
            {
                if (Tracks.Count >= Playlist.Count)
                    return;
                VisibleLoadingMore = Visibility.Visible;
                var response = await vkService.AudioGetAsync(Playlist.Id, Playlist.OwnerId, Playlist.AccessKey, Tracks.Count, 40);

                void Add()
                {
                    foreach (var item in response.Items)
                    {
                        Tracks.Add(item);
                    }
                }

                if (Application.Current.Dispatcher.CheckAccess())
                    Add();
                else
                    await Application.Current.Dispatcher.InvokeAsync(Add);
                VisibleLoadingMore = Visibility.Collapsed;
            }catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("Fatal error in load playlist");
                logger.Error(ex, ex.Message);

                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог загрузить плейлист, попробуйте ещё раз");

                VisibleLoadingMore = Visibility.Collapsed;
            }
        }
        

        public async Task LoadPlaylist(Playlist playlist, bool delete = true)
        {
            try
            {
                logger.Info("Load playlist");
                PlaylistData = new(playlist.Id, playlist.OwnerId, playlist.AccessKey);
                if (delete && Tracks.Count > 0)
                {
                    if (Application.Current.Dispatcher.CheckAccess())
                        Tracks.Clear();
                    else
                        await Application.Current.Dispatcher.InvokeAsync(Tracks.Clear);
                }
                VisibleContent = Visibility.Collapsed;
                VisibleLoading = Visibility.Visible;

                var p = await vkService.GetPlaylistAsync(40, playlist.Id, playlist.AccessKey, playlist.OwnerId);
                this.PlaylistLoaded.Invoke(this, p.Playlist);


                if (p.Playlist.MainArtists.Count == 0)
                {
                    if(p.Playlist.OwnerId < 0)
                    {
                        if(p.Groups != null)
                        {
                            p.Playlist.OwnerName = p.Groups[0].Name;
                            
                        }
                    }
                }
                playlist = p.Playlist;
                playlist.Audios = p.Audios;
                Tracks.ReplaceRange(p.Audios); 

                this.Playlist = playlist;
                Title = playlist.Title;
                Year = playlist.Year.ToString();
                Description = playlist.Description;
               
                var genres = string.Empty;
                logger.Info($"load playlist {Playlist.Genres.Count} genres ");
                foreach (var genre in Playlist.Genres)
                {
                    genres += $"{genre.Name}, ";
                }

                if(Playlist.Genres.Count > 0)
                {
                    Genres = genres.Remove(genres.Length - 2);
                }
               
                if (playlist.Cover != null)
                {
                    Cover = playlist.Cover;

                }

                if (playlist.Year == 0)
                {
                    var date = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(playlist.UpdateTime);
                    Year = $"Обновлен {date.ToString("dd MMMM")}";
                    Genres = "Подборка";
                    //VisibileAddInfo = Visibility.Collapsed;
                }

                logger.Info($"load {Playlist.MainArtists} artists playlist");
                if (Playlist.MainArtists.Count > 0)
                {
                    string s = string.Empty;
                    foreach (var trackArtist in Playlist.MainArtists)
                    {
                        s += trackArtist.Name + ", ";
                    }

                    var artists = s.Remove(s.Length - 2);

                    ArtistText = artists;
                }
                else
                {
                    ArtistText = playlist.OwnerName;
                }


                if (playlist.Audios.Count == 0)
                {
                    logger.Info("load playlist audios");
                    var res = await vkService.AudioGetAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey).ConfigureAwait(false);

                    Tracks.AddRange(res.Items);
                }

                if(playlist.Plays > 1000 && playlist.Plays < 1000000)
                {
                    this.Plays = Math.Round(playlist.Plays / 1000d, 2) + "К";
                }else if(playlist.Plays > 1000000 )
                {
                    this.Plays = Math.Round(playlist.Plays / 1000000d, 2) + "М";

                }else
                {
                    this.Plays = playlist.Plays.ToString();
                }

                

                VisibleContent = Visibility.Visible;
                VisibleLoading = Visibility.Collapsed;

                this.PlaylistLoaded?.Invoke(this, playlist);

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

                logger.Error("Fatal error in load playlist");
                logger.Error(ex, ex.Message);

                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог загрузить контент");

                PlaylistNotLoaded?.Invoke(this, this.Playlist);

            }
        }

        public async Task LoadPlaylistFromData(long playlistId, long ownerId, string accessKey)
        {
            try
            {
                logger.Info($"Load playlist from data: playlistId = {playlistId}, ownerId = {ownerId}, accessKey = {accessKey}");

                await this.LoadPlaylist(new Playlist() { Id = playlistId, AccessKey = accessKey, OwnerId = ownerId }, true);
                //VisibleContent = Visibility.Collapsed;
                //VisibleLoading = Visibility.Visible;
                //Changed("VisibleContent");
                //Changed("VisibleLoading");

            }catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("Fatal error in load playlist from data");
                logger.Error(ex, ex.Message);

                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог загрузить контент");

            }

        }
        public Task LoadPlaylistFromData(PlaylistData data)
        {
            var (playlistId, ownerId, accessKey) = data;
            return LoadPlaylistFromData(playlistId, ownerId, accessKey);
        }

        public async Task<bool> AddPlaylist()
        {
            try
            {
                await vkService.AddPlaylistAsync(Playlist.Id, Playlist.OwnerId, Playlist.AccessKey);
                return true;
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

                logger.Error("Error in add playlist");
                logger.Error(ex, ex.Message);
                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог добавить плейлист");

                return false;
            }
        }

        public async Task<bool> RemovePlaylist()
        {
            try
            {
                await vkService.DeletePlaylistAsync(Playlist.Id, Playlist.OwnerId);
                return true;
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

                logger.Error("Error in remove playlist");
                logger.Error(ex, ex.Message);
                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог удалить плейлист");

                return false;
            }
        }
    }
}
