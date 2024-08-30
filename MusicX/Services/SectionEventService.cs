using System;

namespace MusicX.Services;

public class SectionEventService
{
    public event EventHandler<string>? Event;

    public void Dispatch(object? sender, string name)
    {
        Event?.Invoke(sender, name);
    }
}

public static class SectionEvent
{
    public const string AudiosAdd = "music_audios_add";
    public const string AudiosRemove = "music_audios_remove";
    
    public const string PlaylistsAdd = "music_playlists_add";
    public const string PlaylistsRemove = "music_playlists_remove";
    
    public const string PlaylistsFollow = "music_playlists_follow";
    public const string PlaylistsUnfollow = "music_playlists_unfollow";
    
    public const string ArtistSubscribe = "artist_subscribe";
    public const string ArtistUnsubscribe = "artist_unsubscribe";
}