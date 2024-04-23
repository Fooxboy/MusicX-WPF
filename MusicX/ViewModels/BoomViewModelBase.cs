using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Exceptions.Boom;
using MusicX.Core.Models.Boom;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Models;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using NLog;
using Wpf.Ui;

namespace MusicX.ViewModels;

public abstract class BoomViewModelBase : BaseViewModel
{
    protected readonly BoomService BoomService;
    protected readonly ConfigService ConfigService;
    protected readonly VkService VkService;
    protected readonly Logger Logger;
    protected readonly ISnackbarService SnackbarService;
    protected readonly PlayerService PlayerService;

    protected BoomViewModelBase(BoomService boomService, ConfigService configService, VkService vkService, Logger logger,
        ISnackbarService snackbarService, PlayerService playerService)
    {
        BoomService = boomService;
        ConfigService = configService;
        VkService = vkService;
        Logger = logger;
        SnackbarService = snackbarService;
        PlayerService = playerService;
    }

    public bool IsLoaded { get; set; }
    public bool IsLoadingMix { get; set; }
    public ObservableRangeCollection<Artist> Artists { get; set; } = new();
    public ObservableRangeCollection<Tag> Tags { get; set; } = new();
    public Artist? SelectedArtist { get; set; }
    public Tag? SelectedTag { get; set; }

    public virtual async Task ArtistSelected()
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
            var radioByArtist = await BoomService.GetArtistMixAsync(SelectedArtist.ApiId);

            await PlayerService.PlayAsync(new RadioPlaylist(BoomService, new(radioByArtist, BoomRadioType.Artist)), radioByArtist.Tracks[0].ToTrack());

            IsLoadingMix = false;
        }catch(UnauthorizedException ex)
        {
            Logger.Error("Boom unauthorizedException");
            Logger.Info("Попытка заново получить токен...");

            var config = await ConfigService.GetConfig();
            await AuthBoomAsync(config);

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

            SnackbarService.ShowException("Ошибка загрузки микса", "Мы не смогли загрузить микс, попробуйте ещё раз");
            Logger.Error(ex, ex.Message);
        }
    }

    public virtual async Task TagSelected()
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
            Analytics.TrackEvent("Play Tag Mix", properties);

            if (SelectedTag == null) return;

            IsLoadingMix = true;

            var radio = await BoomService.GetTagMixAsync(SelectedTag.ApiId);

            await PlayerService.PlayAsync(new RadioPlaylist(BoomService, new(radio, BoomRadioType.Tag)), radio.Tracks[0].ToTrack());

            IsLoadingMix = false;
        }
        catch (UnauthorizedException)
        {
            Logger.Error("Boom unauthorizedException");
            Logger.Info("Попытка заново получить токен...");

            var config = await ConfigService.GetConfig();
            await AuthBoomAsync(config);

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

            SnackbarService.ShowException("Ошибка загрузки микса", "Мы не смогли загрузить микс, попробуйте ещё раз");

            Logger.Error(ex, ex.Message);
        }
           
    }

    protected async Task AuthBoomAsync(ConfigModel config)
    {
        try
        {
            var boomVkToken = await VkService.GetBoomToken();

            var boomToken = await BoomService.AuthByTokenAsync(boomVkToken.Token, boomVkToken.Uuid);

            config.BoomToken = boomToken.AccessToken;
            config.BoomTokenTtl = DateTimeOffset.Now + TimeSpan.FromSeconds(boomToken.ExpiresIn);
            config.BoomUuid = boomVkToken.Uuid;

            await ConfigService.SetConfig(config);

            BoomService.SetToken(boomToken.AccessToken);
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

            SnackbarService.ShowException("Ошибка загрузки", "Мы не смогли авторизоваться в ВК Музыке, попробуйте ещё раз");

            IsLoaded = true;

            Logger.Error(ex, ex.Message);

        }
    }
}