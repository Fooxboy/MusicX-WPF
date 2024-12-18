namespace MusicX.ViewModels;

public class MiniAppViewModel(string appId, string url) : BaseViewModel
{
    public string AppId { get; } = appId;
    public string Url { get; } = url;

    public bool IsLoading { get; set; } = true;
    
    public string? ErrorMessage { get; set; }
    public string ErrorDetails { get; set; } = string.Empty;
}