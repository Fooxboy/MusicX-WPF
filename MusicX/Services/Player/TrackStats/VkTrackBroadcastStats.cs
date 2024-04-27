using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Models.Enums;
using MusicX.Shared.Player;
using NLog;
using Wpf.Ui;

namespace MusicX.Services.Player.TrackStats;

public class VkTrackBroadcastStats : ITrackStatsListener
{
    private readonly VkService _vkService;
    private readonly ConfigService _configService;
    private readonly Logger logger;
    private readonly ISnackbarService _snackbarService;

    public VkTrackBroadcastStats(VkService vkService, ConfigService configService, Logger logger,
        ISnackbarService snackbarService)
    {
        _vkService = vkService;
        _configService = configService;
        this.logger = logger;
        _snackbarService = snackbarService;
    }

    public Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason, TimeSpan? position = null)
    {
        try
        {
            if (newTrack.Data is VkTrackData data && _configService.Config.BroadcastVK == true)
                return _vkService.SetBroadcastAsync(new()
                {
                    Id = data.Info.Id,
                    OwnerId = data.Info.OwnerId,
                    AccessKey = data.Info.AccessKey
                });
            return Task.CompletedTask;

        }catch(Exception ex)
        {

            var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
            Crashes.TrackError(ex, properties);

            logger.Error("Fatal error in load playlist");
            logger.Error(ex, ex.Message);

            _snackbarService.ShowException("Произошла ошибка", $"MusicX не смог установить муызку в статус ВКонтакте: {ex.Message}");

            return Task.CompletedTask;
        }
        
    }

    public Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused)
    {
        try
        {
            if (track is null) return Task.CompletedTask;

            if (track.Data is VkTrackData data && _configService.Config.BroadcastVK == true)
                return _vkService.SetBroadcastAsync(paused ? null : new()
                {
                    Id = data.Info.Id,
                    OwnerId = data.Info.OwnerId,
                    AccessKey = data.Info.AccessKey
                });
            return Task.CompletedTask;
        }catch(Exception ex)
        {
            var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
            Crashes.TrackError(ex, properties);

            logger.Error("Fatal error in load playlist");
            logger.Error(ex, ex.Message);

            _snackbarService.ShowException("Произошла ошибка", $"MusicX не смог установить музыку в статус ВКонтакте: {ex.Message}");

            return Task.CompletedTask;
        }
        
    }
}