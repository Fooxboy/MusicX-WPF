using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Exceptions.Boom;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using NLog;
using Wpf.Ui;

namespace MusicX.ViewModels
{
    public class VKMixViewModel : BoomViewModelBase
    {
        public VKMixViewModel(BoomService boomService, ConfigService configService, VkService vkService, Logger logger,
            ISnackbarService snackbarService, PlayerService playerService) :
            base(boomService, configService, vkService, logger, snackbarService, playerService)
        {
            PlayPersonalMixCommand = new AsyncCommand(PlayPersonalMixAsync);
        }

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

            Logger.Info("Открытие страницы VK Mix");
            var config = await ConfigService.GetConfig();

            if (string.IsNullOrEmpty(config.BoomToken) || config.BoomTokenTtl <= DateTimeOffset.Now)
            {
                await AuthBoomAsync(config);
            }
            
            Logger.Info("Авторизация VK Mix уже была пройдена, загрузка...");
            BoomService.SetToken(config.BoomToken);
            await LoadMixesAsync();
        }

        public override Task ArtistSelected()
        {
            PlayingPersonalMix = false;
            return base.ArtistSelected();
        }

        public override Task TagSelected()
        {
            PlayingPersonalMix = false;
            return base.TagSelected();
        }

        private async Task LoadMixesAsync()
        {
            try
            {
                Logger.Info("Загрузка VK Mix...");

                var artists = await BoomService.GetArtistsAsync();
                var tags = await BoomService.GetTagsAsync();
                
                Artists.ReplaceRange(artists);
                Tags.ReplaceRange(tags);

                IsLoaded = true;
                
            }catch(UnauthorizedException)
            {
                Logger.Error("Boom unauthorizedException");
                Logger.Info("Попытка заново получить токен...");

                var config = await ConfigService.GetConfig();
                await AuthBoomAsync(config);

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

                SnackbarService.ShowException("Ошибка загрузки микса", "Мы не смогли загрузить микс, попробуйте ещё раз");

                IsLoaded = true;

                Logger.Error(ex, ex.Message);
            }
        }

        private async Task PlayPersonalMixAsync()
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
                Analytics.TrackEvent("Play Personal Mix", properties);

                if (PlayingPersonalMix)
                {
                    PlayerService.Pause();
                    return;
                }

                PlayingPersonalMix = true;
                IsLoadingMix = true;
                var personalMix =  await BoomService.GetPersonalMixAsync();

                await PlayerService.PlayAsync(new RadioPlaylist(BoomService, personalMix, BoomRadioType.Personal), personalMix.Tracks[0].ToTrack());


                IsLoadingMix = false;
            }
            catch (UnauthorizedException)
            {

               
                Logger.Error("Boom UnauthorizedException");
                Logger.Info("Попытка заново получить токен...");

                var config = await ConfigService.GetConfig();
                await AuthBoomAsync(config);

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


                SnackbarService.ShowException("Ошибка загрузки микса", "Мы не смогли загрузить микс, попробуйте ещё раз");

                Logger.Error(ex, ex.Message);
            }
        }
    }
}
