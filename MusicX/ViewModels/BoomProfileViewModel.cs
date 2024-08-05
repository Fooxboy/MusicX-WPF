using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Exceptions.Boom;
using MusicX.Core.Models.Boom;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;
using NLog;
using Wpf.Ui;

namespace MusicX.ViewModels
{
    public class BoomProfileViewModel : BoomViewModelBase
    {
        public string ProfileBackground { get; set; }

        public string ProfileAvatar { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int CountTracks { get; set; }

        public int CountPlaylists { get; set; }

        public int CountAlbums { get; set; }

        public int CountFriends { get; set; }

        public int CountArtists { get; set; }

        public ObservableRangeCollection<Tag> Tags { get; set; } = new();

        public ObservableRangeCollection<Artist> Artists { get; set; } = new();
        public ObservableRangeCollection<PlaylistTrack> Tracks { get; set; } = new();

        public BoomProfileViewModel(BoomService boomService, ConfigService configService, VkService vkService,
            Logger logger, ISnackbarService snackbarService,
            PlayerService playerService) :
            base(boomService, configService, vkService, logger, snackbarService, playerService)
        {
        }
        
        public async Task OpenProfile()
        {
            try
            {
                IsLoaded = false;

                Tags.Clear();
                Artists.Clear();
                Tracks.Clear();

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Analytics.TrackEvent("Open Boom profile", properties);
                
                var connectionService = StaticService.Container.GetRequiredService<BackendConnectionService>();
                connectionService.ReportMetric("OpenBoomProfile");

                Logger.Info("Открытие страницы Boom profile");
                var config = await ConfigService.GetConfig();

                if (!string.IsNullOrEmpty(config.BoomToken) && config.BoomTokenTtl > DateTimeOffset.Now)
                {
                    Logger.Info("Авторизация Boom уже была пройдена, загрузка...");
                    BoomService.SetToken(config.BoomToken);
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

                SnackbarService.ShowException("Ошибка загрузки", "Мы не смогли открыть Ваш профиль  в ВК музыке");

                IsLoaded = true;

                Logger.Error(ex, ex.Message);
            }
        }

        public async Task LoadProfile()
        {
            try
            {
                var profile = await BoomService.GetUserInfoAsync();

                var topTracks = await BoomService.GetUserTopTracks();

                var topArtists = await BoomService.GetUserTopArtists();

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
                
                Tags.ReplaceRange(profile.Tags);
                Artists.ReplaceRange(topArtists);
                Tracks.ReplaceRange(topTracks.Select(TrackExtensions.ToTrack));
            }
            catch (UnauthorizedException)
            {
                var config = await ConfigService.GetConfig();

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

                SnackbarService.ShowException("Ошибка загрузки", "Мы не смогли открыть Ваш профиль в ВК музыке");

                IsLoaded = true;

                Logger.Error(ex, ex.Message);
            }
            
        }
    }
}
