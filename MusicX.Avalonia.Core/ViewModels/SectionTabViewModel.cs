using DynamicData;
using DynamicData.Binding;
using MusicX.Avalonia.Core.Blocks;
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

    public IObservableCollection<BlockBase> Blocks { get; } = new ObservableCollectionExtended<BlockBase>();

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
        
        Blocks.AddRange(BlockMapper.MapBlocks(section));
    }
}