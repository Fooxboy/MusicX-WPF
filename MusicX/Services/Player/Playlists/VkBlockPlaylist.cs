using System.Collections.Generic;
using System.Linq;
using MusicX.Core.Services;
using MusicX.Helpers;

namespace MusicX.Services.Player.Playlists;

public class VkBlockPlaylist : PlaylistBase<string>
{
    private readonly VkService _vkService;

    public VkBlockPlaylist(VkService vkService, string blockId, bool loadOther = true)
    {
        _vkService = vkService;
        Data = blockId;
        _canLoad = loadOther;
    }

    public override IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        _canLoad = false;
        return _vkService.LoadFullAudiosAsync(Data).Select(TrackExtensions.ToTrack);
    }

    private bool _canLoad;
    public override bool CanLoad => _canLoad;
    public override string Data { get; }
}