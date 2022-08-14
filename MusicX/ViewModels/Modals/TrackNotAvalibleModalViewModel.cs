using System.Linq;
using System.Threading.Tasks;
using MusicX.Core.Services;
namespace MusicX.ViewModels.Modals;

public class TrackNotAvalibleModalViewModel : BaseViewModel
{
    public bool IsLoading { get; set; }
    public bool IsLoaded { get; set; }

    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = "Трек недоступен";
    public string Description { get; set; } = "Хуй его знает почему.";
    
    private readonly VkService _vkService;
    public TrackNotAvalibleModalViewModel(VkService vkService)
    {
        _vkService = vkService;
    }

    public async Task LoadAsync(string trackCode, string audioId)
    {
        IsLoading = true;
        
        var res = await _vkService.AudioGetRestrictionPopup(trackCode, audioId);
        
        if (res.Icons.FirstOrDefault() is { } icon)
            Url = icon.Url;
        
        Title = res.Title;
        Description = res.Text;
        
        IsLoaded = true;
        IsLoading = false;
    }
}
