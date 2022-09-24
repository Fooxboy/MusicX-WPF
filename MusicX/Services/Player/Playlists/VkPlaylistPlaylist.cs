using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.ViewModels;

namespace MusicX.Services.Player.Playlists;

public class VkPlaylistPlaylist : PlaylistBase<PlaylistData>
{
    private readonly VkService _vkService;

    public VkPlaylistPlaylist(VkService vkService, PlaylistData data)
    {
        _vkService = vkService;
        Data = data;
    }

    public override async IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        _canLoad = false;
        
        var (id, ownerId, accessKey) = Data;
        var playlist = await _vkService.LoadFullPlaylistAsync(id, ownerId, accessKey);
        
        foreach (var audio in playlist.Playlist.Audios)
        {
            yield return audio.ToTrack();
        }
    }

    private bool _canLoad = true;
    public override bool CanLoad => _canLoad;
    public override PlaylistData Data { get; }
}