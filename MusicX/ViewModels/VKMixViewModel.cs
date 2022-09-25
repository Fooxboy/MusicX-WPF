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

namespace MusicX.ViewModels
{
    public class VKMixViewModel : BaseViewModel
    {
        private readonly BoomService boomSerivce;
        private readonly ConfigService configService;
        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly PlayerService playerService;

        public VKMixViewModel(BoomService boomSerivce, VkService vkService, ConfigService configService, Logger logger, PlayerService playerService)
        {
            this.boomSerivce = boomSerivce;
            IsLoaded = false;
            this.vkService = vkService;
            this.configService = configService;
            this.logger = logger;
            this.playerService = playerService;

            this.PlayPersonalMixCommand = new AsyncCommand(PlayPersonalMixAsync);
        }

        public bool IsLoaded { get; set; }

        public bool IsLoadingMix { get; set; }

        public ObservableCollection<Artist> Artists { get; set; } = new ObservableCollection<Artist>();

        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public Artist SelectedArtist { get; set; }

        public Tag SelectedTag { get; set; }

        public ICommand PlayPersonalMixCommand { get; set; }

        public async Task OpenedMixesAsync()
        {
            logger.Info("Открытие страницы VK Mix");
            var config = await configService.GetConfig();

            if(!string.IsNullOrEmpty(config.BoomToken))
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
            if (SelectedArtist == null) return;
            IsLoadingMix = true;
            Changed("IsLoadingMix");
            var radioByArtist = await boomSerivce.GetArtistMixAsync(SelectedArtist.ApiId);

            await playerService.PlayAsync(new RadioPlaylist(boomSerivce, radioByArtist, BoomRadioType.Artist), radioByArtist.Tracks[0].ToTrack());

            IsLoadingMix = false;

            Changed("IsLoadingMix");

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
            }catch(UnauthorizedException ex)
            {
                logger.Error("Boom unauthorizedException");
                logger.Info("Попытка заново получить токен...");

                var config = await configService.GetConfig();
                await this.AuthBoomAsync(config);

                await LoadMixesAsync();
            }
            catch (Exception ex)
            {
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
                config.BoomUuid = boomVkToken.Uuid;

                await configService.SetConfig(config);

                boomSerivce.SetToken(boomToken.AccessToken);

                await LoadMixesAsync();
            }
            catch (Exception ex)
            {
                IsLoaded = true;

                Changed("IsLoaded");

                logger.Error(ex, ex.Message);

            }
        }

        private async Task PlayPersonalMixAsync()
        {
            try
            {
                IsLoadingMix = true;
                Changed("IsLoadingMix");
                var personalMix =  await boomSerivce.GetPersonalMixAsync();

                await playerService.PlayAsync(new RadioPlaylist(boomSerivce, personalMix, BoomRadioType.Personal), personalMix.Tracks[0].ToTrack());


                IsLoadingMix = false;
                Changed("IsLoadingMix");

            }
            catch (UnauthorizedException ex)
            {
                logger.Error("Boom UnauthorizedException");
                logger.Info("Попытка заново получить токен...");

                var config = await configService.GetConfig();
                await this.AuthBoomAsync(config);

                await PlayPersonalMixAsync();
            }
        }
    }
}
