using System.Collections.Generic;
using System.Linq;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

public class ListPlaylist : PlaylistBase<IReadOnlyList<PlaylistTrack>>
{
    private bool _canLoad = true;

    public override bool CanLoad => _canLoad;

    public override IReadOnlyList<PlaylistTrack> Data { get; }

    public ListPlaylist(IReadOnlyList<PlaylistTrack> data)
    {
        Data = data;
    }

    public override IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        _canLoad = false;
        return Data.ToAsyncEnumerable();
    }
}