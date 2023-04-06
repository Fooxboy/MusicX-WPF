using DynamicData;
using DynamicData.Binding;
using MusicX.Avalonia.Core.Extensions;
using MusicX.Avalonia.Core.Models;
using VkApi;

namespace MusicX.Avalonia.Core.ViewModels;

public class SectionTabViewModel : MenuTabViewModel
{
    private readonly Api _api;
    private string _title = string.Empty;
    public override string Title => _title;

    public string Id { get; set; } = string.Empty;

    public IObservableCollection<SectionBlock> Blocks { get; } = new ObservableCollectionExtended<SectionBlock>();

    public SectionTabViewModel(Api api)
    {
        _api = api;
    }

    public void Init(string id, string title)
    {
        _title = title;
        Id = id;
    }

    public async Task LoadAsync()
    {
        var section = await _api.GetCatalogSectionAsync(new(Id, null, null, null, null, null, null, null, null));
        
        Blocks.AddRange(section.Section.Blocks);
    }
}