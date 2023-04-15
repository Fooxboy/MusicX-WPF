using System.ComponentModel;
using System.Runtime.CompilerServices;
using MusicX.Avalonia.Core.Extensions;
using MusicX.Shared.Player;
using VkApi;

namespace MusicX.Avalonia.Audio.Playlists;

public class BlockPlaylist : IPlaylist, INotifyPropertyChanged
{
    private readonly Api _api;
    private readonly string _blockId;
    private string? _nextFrom;

    public BlockPlaylist(Api api, string blockId)
    {
        _api = api;
        _blockId = blockId;
    }

    public string? Title { get; set; }

    public bool CanGetChunk => _nextFrom is not null;

    public async ValueTask<IEnumerable<PlaylistTrack>> GetNextChunkAsync(CancellationToken token)
    {
        var response =
            await _api.GetCatalogSectionAsync(new(_blockId, null, _nextFrom, null, null, null, null, null, null));

        Title = response.Section.Title;
        OnPropertyChanged(nameof(Title));

        _nextFrom = response.Section.NextFrom;

        return response.Audios.Select(TrackExtensions.ToTrack);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}