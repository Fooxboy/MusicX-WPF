using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Exceptions.Boom;
using MusicX.Core.Models;
using MusicX.Core.Models.Boom;
using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artist = MusicX.Core.Models.Boom.Artist;

namespace MusicX.ViewModels
{
    public class BoomProfileViewModel : BaseViewModel
    {
        private readonly BoomService boomService;

        private readonly ConfigService configService;
        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly PlayerService playerService;
        private readonly NotificationsService notificationsService;
        public bool IsLoaded { get; set; }

        public string ProfileBackground { get; set; }

        public string ProfileAvatar { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int CountTracks { get; set; }

        public int CountPlaylists { get; set; }

        public int CountAlbums { get; set; }

        public int CountFriends { get; set; }

        public int CountArtists { get; set; }

        public Artist SelectedArtist { get; set; }

        public bool IsLoadingMix { get; set; }

        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public ObservableCollection<Artist> Artists { get; set; } = new ObservableCollection<Artist>();
        public ObservableCollection<Audio> Tracks { get; set; } = new ObservableCollection<Audio>();

        public BoomProfileViewModel(BoomService boomService, VkService vkService, ConfigService configService, Logger logger, PlayerService playerService, NotificationsService notificationsService)
        {
            this.vkService = vkService;
            this.configService = configService;
            this.logger = logger;
            this.playerService = playerService;
            this.notificationsService = notificationsService;
            this.boomService = boomService;
        }


        public async Task OpenProfile()
        {
            try
            {
                IsLoaded = false;

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Analytics.TrackEvent("Open Boom profile", properties);

                logger.Info("Открытие страницы Boom profile");
                var config = await configService.GetConfig();

                if (!string.IsNullOrEmpty(config.BoomToken) && config.BoomTokenTtl > DateTimeOffset.Now)
                {
                    logger.Info("Авторизация Boom уже была пройдена, загрузка...");
                    boomService.SetToken(config.BoomToken);
                    try
                    {
                        await LoadProfile();

                    }catch(UnauthorizedException)
                    {
                        await AuthBoomAsync(config);
                        await LoadProfile();
                    }
                }
                else
                {
                    await AuthBoomAsync(config);

                    await LoadProfile();
                }

                IsLoaded = true;

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

                notificationsService.Show("Ошибка загрузки", "Мы не смогли открыть Ваш профиль  в ВК музыке");

                IsLoaded = true;

                logger.Error(ex, ex.Message);
            }
        }

        public async Task LoadProfile()
        {
            try
            {
                var profile = await boomService.GetUserInfoAsync();

                var topTracks = await boomService.GetUserTopTracks();

                var topArtists = await boomService.GetUserTopArtists();

                topArtists = topArtists.Take(15).ToList();

                topTracks = topTracks.Take(15).ToList();

                //await boomService.GetUserTopPlaylists();

                FirstName = profile.FirstName;
                LastName = profile.LastName;
                ProfileBackground = profile.Avatar.Url;
                ProfileAvatar = profile.Cover.Url;
                CountAlbums = profile.Counts.Album;
                CountTracks = profile.Counts.Track;
                CountArtists = profile.Counts.Artist;
                CountFriends = profile.Counts.Friend;
                CountPlaylists = profile.Counts.Playlist;

                foreach (var tag in profile.Tags)
                {
                    Tags.Add(tag);
                }

                foreach (var artist in topArtists)
                {
                    Artists.Add(artist);
                }

                foreach(var track in topTracks)
                {
                    var audio = new Audio();

                    audio.Title = track.Name;
                    audio.Artist = track.Artist.Name;
                    audio.Url = track.File;
                    audio.Duration = track.Duration;
                    audio.IsAvailable = true;
                    audio.AccessKey = track.GetHashCode().ToString();
                    audio.Id = new Random().Next(0,100);
                    audio.OwnerId = new Random().Next(0, 100);
                    audio.Album = new Core.Models.Album() { Thumb = new Photo() { Photo68 = track.Cover.Url }, 
                        OwnerId = new Random().Next(0, 100), 
                        AccessKey = this.GetHashCode().ToString(), Id = new Random().Next(0,100), Title = "bruh" };

                    Tracks.Add(audio);
                }

            }
            catch (UnauthorizedException)
            {
                var config = await configService.GetConfig();

                await AuthBoomAsync(config);
                await LoadProfile();
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

                notificationsService.Show("Ошибка загрузки", "Мы не смогли открыть Ваш профиль в ВК музыке");

                IsLoaded = true;

                logger.Error(ex, ex.Message);
            }
            
        }

        private async Task AuthBoomAsync(ConfigModel config)
        {
            try
            {
                var boomVkToken = await vkService.GetBoomToken();

                var boomToken = await boomService.AuthByTokenAsync(boomVkToken.Token, boomVkToken.Uuid);

                config.BoomToken = boomToken.AccessToken;
                config.BoomTokenTtl = DateTimeOffset.Now + TimeSpan.FromSeconds(boomToken.ExpiresIn);
                config.BoomUuid = boomVkToken.Uuid;

                await configService.SetConfig(config);

                boomService.SetToken(boomToken.AccessToken);

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

                notificationsService.Show("Ошибка загрузки", "Мы не смогли авторизоваться в ВК Музыке, попробуйте ещё раз");

                IsLoaded = true;

                logger.Error(ex, ex.Message);

            }
        }

        public async Task ArtistSelected()
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Analytics.TrackEvent("Play Artist Mix", properties);

                if (SelectedArtist == null) return;
                IsLoadingMix = true;
                var radioByArtist = await boomService.GetArtistMixAsync(SelectedArtist.ApiId);

                await playerService.PlayAsync(new RadioPlaylist(boomService, radioByArtist, BoomRadioType.Artist), radioByArtist.Tracks[0].ToTrack());

                IsLoadingMix = false;
            }
            catch (UnauthorizedException ex)
            {
                logger.Error("Boom unauthorizedException");
                logger.Info("Попытка заново получить токен...");

                var config = await configService.GetConfig();
                await this.AuthBoomAsync(config);

                await ArtistSelected();
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

                notificationsService.Show("Ошибка загрузки микса", "Мы не смогли загрузить микс, попробуйте ещё раз");
                this.logger.Error(ex, ex.Message);
            }
        }
    }
}
