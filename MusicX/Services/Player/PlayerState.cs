using System;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;

namespace MusicX.Services.Player;

public record PlayerState(IPlaylist Playlist, PlaylistTrack Track, TimeSpan Position)
{
    public static PlayerState? CreateOrNull(PlayerService service) =>
        service is { CurrentPlaylist: null } or { CurrentTrack: null }
            ? null
            : new(service.CurrentPlaylist, service.CurrentTrack, service.Position);
}