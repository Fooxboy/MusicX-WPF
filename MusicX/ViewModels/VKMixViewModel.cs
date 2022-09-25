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
        }

        public bool IsLoaded { get; set; }

        public ObservableCollection<Artist> Artists { get; set; } = new ObservableCollection<Artist>();

        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public Artist SelectedArtist { get; set; }

        public Tag SelectedTag { get; set; }

        public async Task OpenedMixesAsync()
        {
            logger.Info("Открытие страницы VK Mix");
            var config = await configService.GetConfig();

            if(!string.IsNullOrEmpty(config.BoomToken) && config.BoomTokenTtl < DateTimeOffset.Now)
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
            var radioByArtist = await boomSerivce.GetArtistMixAsync(SelectedArtist.ApiId);

            await playerService.PlayAsync(new RadioPlaylist(boomSerivce, radioByArtist), radioByArtist.Tracks[0].ToTrack());
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
                config.BoomTokenTtl = DateTimeOffset.Now + TimeSpan.FromSeconds(boomToken.ExpiresIn);
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
    }
}
