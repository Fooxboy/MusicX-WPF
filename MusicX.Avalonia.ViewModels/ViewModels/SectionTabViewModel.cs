using System.Reactive;
using DynamicData;
using DynamicData.Binding;
using MusicX.Avalonia.Audio.Playlists;
using MusicX.Avalonia.Audio.Services;
using MusicX.Avalonia.Core.Blocks;
using MusicX.Avalonia.Core.Extensions;
using MusicX.Avalonia.Core.Models;
using ReactiveUI;
using VkApi;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class SectionTabViewModel : MenuTabViewModel, IEquatable<SectionTabViewModel>, IActivatableViewModel
{
    private readonly Api _api;
    private readonly QueueService _queueService;
    private string _title = string.Empty;
    public override string Title => _title;

    public string Id { get; set; } = string.Empty;

    public IObservableCollection<BlockBase> Blocks { get; } = new ObservableCollectionExtended<BlockBase>();
    
    public ReactiveCommand<CatalogAudio, Unit> TrackClickCommand { get; } 

    public SectionTabViewModel(Api api, QueueService queueService)
    {
        _api = api;
        _queueService = queueService;

        TrackClickCommand = ReactiveCommand.CreateFromTask<CatalogAudio>(PlayAsync);
    }

    private Task PlayAsync(CatalogAudio audio, CancellationToken token)
    {
        IPlaylist playlist;
        if (audio.ParentBlockId is null)
            playlist = new SinglePlaylist(audio.ToTrack());
        else
            playlist = new BlockPlaylist(_api, audio.ParentBlockId, Title);

        return _queueService.PlayPlaylistAsync(playlist, token, audio.Url).AsTask();
    }

    public void Init(string id, string title)
    {
        _title = title;
        Id = id;
    }

    public async Task LoadAsync()
    {
        if (Blocks.Count > 0)
            return;
        var section = await _api.GetCatalogSectionAsync(new(Id, null, null, null, null, null, null, null, null));
        
        Blocks.AddRange(BlockMapper.MapBlocks(section));
    }

    public bool Equals(SectionTabViewModel? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SectionTabViewModel)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public ViewModelActivator Activator { get; } = new ViewModelActivator();
}