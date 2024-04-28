using System;
using System.Threading.Tasks;
using MusicX.Core.Services;
using MusicX.Models.Enums;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.TrackStats;

public class DiscordTrackStats : ITrackStatsListener
{
    private readonly DiscordService _discordService;
    private readonly ConfigService _configService;

    public DiscordTrackStats(DiscordService discordService, ConfigService configService)
    {
        _discordService = discordService;
        _configService = configService;
    }

    public Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason, TimeSpan? position = null)
    {
        if (_configService.Config.ShowRPC == true)
            SetTrack(newTrack);
        
        return Task.CompletedTask;
    }

    private void SetTrack(PlaylistTrack track, TimeSpan? position = null)
    {
        _discordService.SetTrackPlay(track.GetArtistsString(), track.Title, track.Data.Duration -
                                                                            (position ?? TimeSpan.Zero),
                                     track.AlbumId?.CoverUrl ?? string.Empty);
    }

    public Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused)
    {
        if (_configService.Config.ShowRPC != true) return Task.CompletedTask;
        
        if (paused)
            _discordService.RemoveTrackPlay(true);
        else
            SetTrack(track, position);

        return Task.CompletedTask;
    }
}