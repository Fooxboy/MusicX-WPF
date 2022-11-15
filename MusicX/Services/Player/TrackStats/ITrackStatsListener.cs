using System;
using System.Threading.Tasks;
using MusicX.Models.Enums;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.TrackStats;

public interface ITrackStatsListener
{
    Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason);
    Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused);
}