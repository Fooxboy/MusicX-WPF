using System;
using System.Linq;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using MusicX.Models.Enums;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.TrackStats;

public class LastFmStats : ITrackStatsListener
{
    private readonly IScrobbler _scrobbler;
    private readonly ITrackApi _trackApi;
    private readonly ConfigService _configService;

    public LastFmStats(IScrobbler scrobbler, ITrackApi trackApi, ConfigService configService)
    {
        _scrobbler = scrobbler;
        _trackApi = trackApi;
        _configService = configService;
    }
    
    public async Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason, TimeSpan? position = null)
    {
        if (_configService.Config.LastFmSession is null || _configService.Config.SendLastFmScrobbles is not true)
            return;
        
        if (previousTrack is not null && previousTrack.Data.Duration > TimeSpan.FromSeconds(30) && 
            position.HasValue && (position.Value > TimeSpan.FromMinutes(4) || position.Value > previousTrack.Data.Duration / 2))
            await _scrobbler.ScrobbleAsync(new Scrobble(previousTrack.MainArtists.First().Name,
                previousTrack.AlbumId?.Name, previousTrack.Title, DateTimeOffset.Now - position.Value)
            {
                Duration = previousTrack.Data.Duration
            });

        await _trackApi.UpdateNowPlayingAsync(new Scrobble(newTrack.MainArtists.First().Name, newTrack.AlbumId?.Name,
            newTrack.Title, DateTimeOffset.Now)
        {
            Duration = newTrack.Data.Duration
        });
    }

    public Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused)
    {
        return Task.CompletedTask;
    }
}