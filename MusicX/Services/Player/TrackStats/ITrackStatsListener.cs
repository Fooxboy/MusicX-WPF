using System;
using System.Threading.Tasks;
using MusicX.Services.Player.Playlists;

namespace MusicX.Services.Player.TrackStats;

public interface ITrackStatsListener
{
    Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason);
    Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused);
}

public enum ChangeReason
{
    TrackEnd,
    TrackChange,
    NextButton,
    PrevButton,
    PlaylistChange,
    Error
}