using System;
using System.Threading.Tasks;
using MusicX.Core.Services;
using MusicX.Models.Enums;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.TrackStats;

public class VkTrackBroadcastStats : ITrackStatsListener
{
    private readonly VkService _vkService;
    private readonly ConfigService _configService;

    public VkTrackBroadcastStats(VkService vkService, ConfigService configService)
    {
        _vkService = vkService;
        _configService = configService;
    }

    public Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason)
    {
        if (newTrack.Data is VkTrackData data && _configService.Config.BroadcastVK == true)
            return _vkService.SetBroadcastAsync(new()
            {
                Id = data.Info.Id,
                OwnerId = data.Info.OwnerId,
                AccessKey = data.Info.AccessKey
            });
        return Task.CompletedTask;
    }

    public Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused)
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
    }
}