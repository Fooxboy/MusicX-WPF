using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using MusicX.Avalonia.Audio.Services;
using MusicX.Avalonia.Core.Blocks;
using MusicX.Avalonia.Core.Extensions;
using ReactiveUI;
using VkApi;
using OneOf;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class SectionTabViewModel : MenuTabViewModel, IEquatable<SectionTabViewModel>, IActivatableViewModel
{
    private readonly Api _api;
    private readonly IQueueService _queueService;
    private string _title = string.Empty;
    private string? _nextFrom;
    public override string Title => _title;

    public override OneOf<string, Symbol> Icon =>
        Title switch
        {
            "Главная" => Symbol.Home,
            "Моя музыка" => PathIcons.MusicNote,
            "Обзор" => PathIcons.Compass,
            "Обновления" => PathIcons.News,
            _ => Symbol.Code
        };

    public string Id { get; set; } = string.Empty;

    public IObservableCollection<BlockBase> Blocks { get; } = new ObservableCollectionExtended<BlockBase>();
    
    public bool IsLoading { get; set; }

    public SectionTabViewModel(Api api, IQueueService queueService)
    {
        _api = api;
        _queueService = queueService;
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
        IsLoading = true;
        var section = await _api.GetCatalogSectionAsync(new(Id, null, null, null, null, null, null, null, null));

        Blocks.AddRange(BlockMapper.MapBlocks(section));
        _nextFrom = section.Section.NextFrom;
        IsLoading = false;
    }
    
    public async Task LoadMoreAsync()
    {
        if (Blocks.Count == 0 || _nextFrom == null)
            return;
        IsLoading = true;
        var section = await _api.GetCatalogSectionAsync(new(Id, null, _nextFrom, null, null, null, null, null, null));

        Blocks.AddRange(BlockMapper.MapBlocks(section));
        _nextFrom = section.Section.NextFrom;
        IsLoading = false;
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