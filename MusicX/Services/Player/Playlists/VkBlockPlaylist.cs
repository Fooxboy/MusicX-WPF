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
    private string? _nextFrom;
    
    [ActivatorUtilitiesConstructor]
    // ReSharper disable once RedundantOverload.Global
    public VkBlockPlaylist(VkService vkService, string blockId) : this(vkService, blockId, true) {}

    public VkBlockPlaylist(VkService vkService, string blockId, bool loadOther = true)
    {
        _vkService = vkService;
        Data = blockId;
        _firstLoad = loadOther;
    }

    public override async IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        if (_firstLoad || _nextFrom is not null)
        {
            var response = await _vkService.GetSectionAsync(Data, _nextFrom);
            _nextFrom = response.Section?.NextFrom;
            foreach (var item in response.Audios)
            {
                yield return item.ToTrack();
            }
        }
        
        _firstLoad = false;
    }

    private bool _firstLoad;
    public override bool CanLoad => _nextFrom is not null || _firstLoad;
    public override string Data { get; }
}