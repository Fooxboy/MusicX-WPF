using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

[JsonConverter(typeof(PlaylistJsonConverter<VkBlockPlaylist, string>))]
public class VkBlockPlaylist : PlaylistBase<string>
{
    private readonly VkService _vkService;
    
    [ActivatorUtilitiesConstructor]
    // ReSharper disable once RedundantOverload.Global
    public VkBlockPlaylist(VkService vkService, string blockId) : this(vkService, blockId, true) {}

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