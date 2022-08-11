using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Common;
namespace MusicX.ViewModels.Modals;

public class CaptchaViewModel : BaseViewModel
{
    public string CaptchaUrl { get; }
    public TaskCompletionSource<string> ResultSource { get; } = new();
    public ICommand SubmitCommand { get; }

    public CaptchaViewModel(string captchaUrl)
    {
        CaptchaUrl = captchaUrl;
        SubmitCommand = new RelayCommand(Submit);
    }
    private void Submit(object arg)
    {
        if (arg is string code && !string.IsNullOrEmpty(code))
            ResultSource.SetResult(code);
    }
}
