using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class VideoModalViewModel : ModalViewModel
{
    private string _title = "Video";
    public override string Title => _title;

    public async Task LoadVideoAsync(CatalogVideo catalogVideo)
    {
        
    }
}