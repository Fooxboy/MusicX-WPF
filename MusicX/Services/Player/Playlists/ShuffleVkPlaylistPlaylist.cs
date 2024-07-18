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

[JsonConverter(typeof(PlaylistJsonConverter<ShuffleVkPlaylistPlaylist, PlaylistData>))]
public class ShuffleVkPlaylistPlaylist : PlaylistBase<PlaylistData>
{
    private readonly VkService _vkService;
    private int _offset;
    private int _count;

    private ChunkedCollection<PlaylistTrack>? _tracks;

    private const int LoadCount = 40;

    public ShuffleVkPlaylistPlaylist(VkService vkService, PlaylistData data)
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
            _tracks = new(_count, async range =>
            {
                var (offset, length) = range.GetOffsetAndLength(_count);
                var response = await _vkService.AudioGetAsync(id, ownerId, accessKey, offset, length + 1);
                return response.Items.Select(audio => audio.ToTrack(trackPlaylist));
            });

            _firstLoad = false;
        }
        
        var items = await _tracks!.GetRangeAsync(_offset..Math.Min(_count - 1, _offset + LoadCount));

        _offset += Math.Min(_count - _offset - 1, LoadCount);
        
        foreach (var item in items)
        {
            yield return item;
        }
    }

    private bool _firstLoad = true;
    public override bool CanLoad => _firstLoad || _offset < _count;
    public override PlaylistData Data { get; }

    private class ChunkedCollection<T>
    {
        public int Count { get; }

        private readonly Func<Range, ValueTask<IEnumerable<T>>> _loader;
        private readonly int[] _indices;
        private readonly T?[] _items;

        public ChunkedCollection(int count, Func<Range, ValueTask<IEnumerable<T>>> loader)
        {
            Count = count;
            _loader = loader;

            _indices = new int[count];
            _items = new T?[count];

            for (var i = 0; i < count - 1; i++)
            {
                _indices[i] = Random.Shared.Next(i, count);
            }
        }

        public async ValueTask<IEnumerable<T>> GetRangeAsync(Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(Count);
            
            var indicesSegment = new ArraySegment<int>(_indices, offset, length);
            var result = new T[length];
            
            for (var i = 0; i < indicesSegment.Count; i++)
            {
                var index = indicesSegment[i];

                if (_items[index] is null)
                {
                    var rangeToLoad = GetLoadableRangeFromIndex(index);

                    var items = await _loader(rangeToLoad);

                    var (loadOffset, loadLength) = rangeToLoad.GetOffsetAndLength(Count);

                    Array.Copy(items as T[] ?? items.ToArray(), 0, _items, loadOffset, loadLength);
                }
                
                result[i] = _items[index] ?? throw new InvalidOperationException();
            }
            
            return result;
        }

        private Range GetLoadableRangeFromIndex(int index)
        {
            var startIndex = Math.Min(index,
                Array.FindIndex(_items, Math.Clamp(index - 20, 0, Count - 1), static b => b != null));
            int endIndex;

            if (startIndex - index > 15)
            {
                endIndex = startIndex;
                startIndex = index;
            }
            else
            {
                endIndex = Math.Min(index + 20,
                    Array.FindIndex(_items, Math.Clamp(index + 1, 0, Count - 1), static b => b != null));
            }
            
            if (endIndex < 0 && startIndex < 0)
            {
                return Math.Clamp(index - 20, 0, Count - 1)..Math.Clamp(index + 20, 0, Count - 1);
            }

            return startIndex..Math.Max(endIndex, Math.Clamp(index + 20, 0, Count - 1));
        }
    }
}