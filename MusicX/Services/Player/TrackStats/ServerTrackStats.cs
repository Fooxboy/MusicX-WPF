using System;
using System.Threading.Tasks;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.TrackStats;

public class ServerTrackStats : ITrackStatsListener
{
    private readonly ServerService _service;

    public ServerTrackStats(ServerService service)
    {
        _service = service;
    }

    public Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason)
    {
        return _service.OnTrackChangedAsync(newTrack);
    }

    public Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused)
    {
        return _service.OnPlayStateChangedAsync(position, paused);
    }
}