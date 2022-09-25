using MusicX.Core.Models.Boom;
using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.Core.Exceptions.Boom;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;

namespace MusicX.ViewModels
{
    public class VKMixViewModel : BaseViewModel
    {
        private readonly BoomService boomSerivce;
        private readonly ConfigService configService;
        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly PlayerService playerService;
        private readonly NotificationsService notificationsService;

        public VKMixViewModel(BoomService boomSerivce, VkService vkService, ConfigService configService, Logger logger, PlayerService playerService, NotificationsService notificationsService)
        {
            this.boomSerivce = boomSerivce;
            IsLoaded = false;
            this.vkService = vkService;
            this.configService = configService;
            this.logger = logger;
            this.playerService = playerService;

            this.PlayPersonalMixCommand = new AsyncCommand(PlayPersonalMixAsync);
            this.notificationsService = notificationsService;
        }

        public bool IsLoaded { get; set; }

        public bool IsLoadingMix { get; set; }

        public ObservableCollection<Artist> Artists { get; set; } = new ObservableCollection<Artist>();

        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public Artist SelectedArtist { get; set; }

        public Tag SelectedTag { get; set; }

        public ICommand PlayPersonalMixCommand { get; set; }

        public bool PlayingPersonalMix { get; set; }

        public async Task OpenedMixesAsync()
        {
            var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
            Analytics.TrackEvent("Open VK Mix", properties);

            logger.Info("Открытие страницы VK Mix");
            var config = await configService.GetConfig();

            if(!string.IsNullOrEmpty(config.BoomToken) && config.BoomTokenTtl > DateTimeOffset.Now)
            {
                logger.Info("Авторизация VK Mix уже была пройдена, загрузка...");
                boomSerivce.SetToken(config.BoomToken);
                await LoadMixesAsync();
            }else
            {
                await AuthBoomAsync(config);
            }
        }

        public async Task ArtistSelected()
        {
            try
            {
                PlayingPersonalMix = false;
                Changed("PlayingPersonalMix");
                if (SelectedArtist == null) return;
                IsLoadingMix = true;
                Changed("IsLoadingMix");
                var radioByArtist = await boomSerivce.GetArtistMixAsync(SelectedArtist.ApiId);

                await playerService.PlayAsync(new RadioPlaylist(boomSerivce, radioByArtist, BoomRadioType.Artist), radioByArtist.Tracks[0].ToTrack());

                IsLoadingMix = false;

                Changed("IsLoadingMix");
            }catch(UnauthorizedException ex)
            {
                logger.Error("Boom unauthorizedException");
                logger.Info("Попытка заново получить токен...");

                var config = await configService.GetConfig();
                await this.AuthBoomAsync(config);

                await ArtistSelected();
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

                notificationsService.Show("Ошибка загрузки микса", "Мы не смогли загрузить микс, попробуйте ещё раз");
                this.logger.Error(ex, ex.Message);
            }
        }

        public async Task TagSelected()
        {
            try
            {
                if (SelectedTag == null) return;

                PlayingPersonalMix = false;
                Changed("PlayingPersonalMix");
                IsLoadingMix = true;
                Changed("IsLoadingMix");

                var radio = await boomSerivce.GetTagMixAsync(SelectedTag.ApiId);

                await playerService.PlayAsync(new RadioPlaylist(boomSerivce, radio, BoomRadioType.Tag), radio.Tracks[0].ToTrack());

                IsLoadingMix = false;

                Changed("IsLoadingMix");
            }
            catch (UnauthorizedException)
            {
                logger.Error("Boom unauthorizedException");
                logger.Info("Попытка заново получить токен...");

                var config = await configService.GetConfig();
                await this.AuthBoomAsync(config);

                await TagSelected();
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

                logger.Error(ex, ex.Message);
            }
           
        }

        private async Task LoadMixesAsync()
        {
            try
            {
                logger.Info("Загрузка VK Mix...");

                var artists = await boomSerivce.GetArtistsAsync();
                var tags = await boomSerivce.GetTagsAsync();

                foreach (var artist in artists) Artists.Add(artist);
                foreach (var tag in tags) Tags.Add(tag);

                IsLoaded = true;

                Changed("Artists");
                Changed("IsLoaded");
            }catch(UnauthorizedException)
            {
                logger.Error("Boom unauthorizedException");
                logger.Info("Попытка заново получить токен...");

                var config = await configService.GetConfig();
                await this.AuthBoomAsync(config);

                await LoadMixesAsync();
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

                IsLoaded = true;

                Changed("IsLoaded");

                logger.Error(ex, ex.Message);
            }
        }

        private async Task AuthBoomAsync(ConfigModel config)
        {
            try
            {
                var boomVkToken = await vkService.GetBoomToken();

                var boomToken = await boomSerivce.AuthByTokenAsync(boomVkToken.Token, boomVkToken.Uuid);

                config.BoomToken = boomToken.AccessToken;
                config.BoomTokenTtl = DateTimeOffset.Now + TimeSpan.FromSeconds(boomToken.ExpiresIn);
                config.BoomUuid = boomVkToken.Uuid;

                await configService.SetConfig(config);

                boomSerivce.SetToken(boomToken.AccessToken);

                await LoadMixesAsync();
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

                Changed("IsLoaded");

                logger.Error(ex, ex.Message);

            }
        }

        private async Task PlayPersonalMixAsync()
        {
            try
            {
                if (PlayingPersonalMix)
                {
                    playerService.Pause();
                    return;
                }

                PlayingPersonalMix = true;
                Changed("PlayingPersonalMix");
                IsLoadingMix = true;
                Changed("IsLoadingMix");
                var personalMix =  await boomSerivce.GetPersonalMixAsync();

                await playerService.PlayAsync(new RadioPlaylist(boomSerivce, personalMix, BoomRadioType.Personal), personalMix.Tracks[0].ToTrack());


                IsLoadingMix = false;
                Changed("IsLoadingMix");

            }
            catch (UnauthorizedException)
            {

               
                logger.Error("Boom UnauthorizedException");
                logger.Info("Попытка заново получить токен...");

                var config = await configService.GetConfig();
                await this.AuthBoomAsync(config);

                await PlayPersonalMixAsync();
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


                notificationsService.Show("Ошибка загрузки микса", "Мы не смогли загрузить микс, попробуйте ещё раз");

                logger.Error(ex, ex.Message);
            }
        }
    }
}
