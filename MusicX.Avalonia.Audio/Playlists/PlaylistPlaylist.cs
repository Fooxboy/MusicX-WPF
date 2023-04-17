using MusicX.Avalonia.Core.Extensions;
using MusicX.Avalonia.Core.Models;
using MusicX.Shared.Player;
using VkApi;

namespace MusicX.Avalonia.Audio.Playlists;

public class PlaylistPlaylist : IPlaylist
{
    private int _playedCount;
    private readonly Api _api;
    private readonly CatalogPlaylist _playlist;
    private readonly List<PlaylistTrack> _tracks;
    public string? Title => _playlist.Title;

    public bool CanGetChunk => _playedCount < _playlist.Count;
    
    public PlaylistPlaylist(Api api, CatalogPlaylist playlist, IEnumerable<PlaylistTrack>? initialTracks = null)
    {
        _api = api;
        _playlist = playlist;
        _tracks = initialTracks?.ToList() ?? new(_playlist.Count);
    }

    public async ValueTask<IEnumerable<PlaylistTrack>> GetNextChunkAsync(CancellationToken token)
    {
        var skip = _playedCount;
        if (_playedCount + 40 < _tracks.Count)
        {
            _playedCount += 40;
            return _tracks.Skip(skip);
        }

        var response = await _api.GetAudioAsync(new((int?)_playlist.OwnerId, null, _playlist.Id, null, null, null,
                                                    _playedCount, 80, null, null, _playlist.AccessKey, null, null,
                                                    null));
        _tracks.AddRange(response.Items.Select(TrackExtensions.ToTrack));
        
        _playedCount += Math.Min(40, _tracks.Count - _playedCount);
        return _tracks.Skip(skip);
    }
}