using System.Collections.Generic;
using System.Text.Json.Serialization;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Shared.Player;
using MusicX.ViewModels;

namespace MusicX.Services.Player.Playlists;

[JsonConverter(typeof(PlaylistJsonConverter<VkPlaylistPlaylist, PlaylistData>))]
public class VkPlaylistPlaylist : PlaylistBase<PlaylistData>
{
    private readonly VkService _vkService;
    private int _offset;
    private int _count;
    
    private const int LoadCount = 40;

    public VkPlaylistPlaylist(VkService vkService, PlaylistData data)
    {
        _vkService = vkService;
        Data = data;
    }

    public override async IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        var (id, ownerId, accessKey) = Data;
        var trackPlaylist = new Playlist
        {
            Id = Data.PlaylistId,
            OwnerId = Data.OwnerId,
            AccessKey = Data.AccessKey
        };
        
        if (_firstLoad)
        {
            var playlist = await _vkService.GetPlaylistAsync(LoadCount, id, accessKey, ownerId);

            _count = (int)playlist.Playlist.Count;
            _offset = playlist.Audios.Count;

            foreach (var audio in playlist.Audios)
            {
                yield return audio.ToTrack(trackPlaylist);
            }
        
            _firstLoad = false;
            yield break;
        }
        
        var response = await _vkService.AudioGetAsync(id, ownerId, accessKey, _offset, LoadCount);

        _offset += response.Items.Count;
        
        foreach (var audio in response.Items)
        {
            yield return audio.ToTrack(trackPlaylist);
        }
    }

    private bool _firstLoad = true;
    public override bool CanLoad => _firstLoad || _offset < _count;
    public override PlaylistData Data { get; }
}