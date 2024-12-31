using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Shared.Player;
using MusicX.ViewModels;

namespace MusicX.Services.Player.Playlists;

[JsonConverter(typeof(PlaylistJsonConverter<VkPlaylistPlaylist, PlaylistData>))]
public class VkPlaylistPlaylist : PlaylistBase<PlaylistData>, IRandomAccessPlaylist, IShufflePlaylist
{
    private readonly VkService _vkService;
    private int _offset;
    private int _count;

    private int? _seed;
    
    private const int LoadCount = 40;

    public VkPlaylistPlaylist(VkService vkService, PlaylistData data)
    {
        _vkService = vkService;
        Data = data;
    }

    public override async IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        var (id, ownerId, accessKey, _) = Data;
        var trackPlaylist = new Playlist
        {
            Id = Data.PlaylistId,
            OwnerId = Data.OwnerId,
            AccessKey = Data.AccessKey
        };
        
        await PerformFirstLoadAsync();
        
        var response = await _vkService.AudioGetAsync(id, ownerId, accessKey, _offset, LoadCount, _seed);

        _offset += response.Items.Count;
        
        foreach (var audio in response.Items)
        {
            yield return audio.ToTrack(trackPlaylist);
        }
    }

    public IPlaylist ShuffleWithSeed(int seed)
    {
        return new VkPlaylistPlaylist(_vkService, Data)
        {
            _seed = seed
        };
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Data, _seed);
    }

    public override bool Equals(IPlaylist? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;
        
        if (other is not VkPlaylistPlaylist playlist)
            return false;
        
        return Data.Equals(playlist.Data) && _seed == playlist._seed;
    }

    private async ValueTask PerformFirstLoadAsync()
    {
        var (id, ownerId, accessKey, _) = Data;

        if (_firstLoad)
        {
            var playlist = await _vkService.GetPlaylistAsync(0, id, accessKey, ownerId);

            _count = (int)playlist.Playlist.Count;
            _offset = 0;

            _firstLoad = false;
        }
    }

    private bool _firstLoad = true;
    public override bool CanLoad => _firstLoad || _offset < _count;
    public override PlaylistData Data { get; }
    public async ValueTask<int> GetCountAsync()
    {
        if (Data.Count.HasValue)
            return Data.Count.Value;

        await PerformFirstLoadAsync();

        return _count;
    }

    public async ValueTask<IEnumerable<PlaylistTrack>> GetRangeAsync(Range range)
    {
        var (id, ownerId, accessKey, _) = Data;
        var trackPlaylist = new Playlist
        {
            Id = Data.PlaylistId,
            OwnerId = Data.OwnerId,
            AccessKey = Data.AccessKey
        };
        
        await PerformFirstLoadAsync();
        
        var (offset, length) = range.GetOffsetAndLength(_count);
        var response = await _vkService.AudioGetAsync(id, ownerId, accessKey, offset, length, _seed);
        return response.Items.Select(audio => audio.ToTrack(trackPlaylist));
    }
}