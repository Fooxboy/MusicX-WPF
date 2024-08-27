using System;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;

namespace MusicX.Services.Player;

public record PlayerState(IPlaylist Playlist, int CurrentIndex, TimeSpan Position)
{
    public static PlayerState? CreateOrNull(PlayerService service) =>
        service is { CurrentPlaylist: null } or { CurrentTrack: null }
            ? null
            : new(service.CurrentPlaylist, service.CurrentIndex, service.Position);
}