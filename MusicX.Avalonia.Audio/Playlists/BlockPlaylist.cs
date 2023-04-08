using MusicX.Avalonia.Core.Extensions;
using MusicX.Shared.Player;
using VkApi;

namespace MusicX.Avalonia.Audio.Playlists;

public class BlockPlaylist : IPlaylist
{
    private readonly Api _api;
    private readonly string _blockId;
    private string? _nextFrom;

    public BlockPlaylist(Api api, string blockId, string title)
    {
        _api = api;
        _blockId = blockId;
        Title = title;
    }

    public string? Title { get; }

    public bool CanGetChunk => _nextFrom is not null;

    public async ValueTask<IEnumerable<PlaylistTrack>> GetNextChunkAsync(CancellationToken token)
    {
        var response =
            await _api.GetCatalogSectionAsync(new(_blockId, null, _nextFrom, null, null, null, null, null, null));

        _nextFrom = response.Section.NextFrom;

        return response.Audios.Select(TrackExtensions.ToTrack);
    }
}