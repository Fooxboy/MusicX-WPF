using System;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Common;
using Wpf.Ui.Extensions;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.ViewModels.Modals;

public class BrowserCaptchaModalViewModel : BaseViewModel
{
    private readonly NavigationService _navigationService;
    private readonly ISnackbarService _snackbarService;

    public Uri? RedirectUri { get; set; }
    public ICommand CloseCommand { get; }

    public TaskCompletionSource<string?> CompletionSource { get; } = new();

    public BrowserCaptchaModalViewModel(NavigationService navigationService, ISnackbarService snackbarService)
    {
        _navigationService = navigationService;
        _snackbarService = snackbarService;
        CloseCommand = new RelayCommand(ExecuteClose);
    }

    private void ExecuteClose()
    {
        CompletionSource.SetResult(null);
        _navigationService.CloseModal();
    }

    public void Complete(string successToken)
    {
        if (RedirectUri is null) return;
        
        CompletionSource.SetResult(successToken);
        _navigationService.CloseModal();
    }
}