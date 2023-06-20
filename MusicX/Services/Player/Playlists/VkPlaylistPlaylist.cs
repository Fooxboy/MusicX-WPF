using System.Collections.Generic;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Shared.Player;
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

        var trackPlaylist = new Playlist
        {
            Id = Data.PlaylistId,
            OwnerId = Data.OwnerId,
            AccessKey = Data.AccessKey
        };
        
        foreach (var audio in playlist.Audios)
        {
            yield return audio.ToTrack(trackPlaylist);
        }
    }

    private bool _canLoad = true;
    public override bool CanLoad => _canLoad;
    public override PlaylistData Data { get; }
}