using System.Threading.Tasks;
using System.Windows.Input;
using MusicX.Services;
using Wpf.Ui.Common;

namespace MusicX.ViewModels.Modals;

public class CaptchaModalViewModel : BaseViewModel
{
    private readonly NavigationService _navigationService;

    public string ImageUrl { get; set; } = string.Empty;
    public ICommand SolveCommand { get; }
    public ICommand CloseCommand { get; }

    public TaskCompletionSource<string?> CompletionSource { get; } = new();

    public CaptchaModalViewModel(NavigationService navigationService)
    {
        _navigationService = navigationService;
        SolveCommand = new RelayCommand(ExecuteSolve);
        CloseCommand = new RelayCommand(ExecuteClose);
    }

    private void ExecuteClose()
    {
        CompletionSource.SetResult(null);
        _navigationService.CloseModal();
    }

    private void ExecuteSolve(object obj)
    {
        CompletionSource.SetResult(obj as string);
        _navigationService.CloseModal();
    }
}