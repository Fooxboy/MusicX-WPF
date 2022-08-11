using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Common;
namespace MusicX.ViewModels.Login;

public class TwoFactorViewModel
{

    public TwoFactorViewModel()
    {
        SubmitCommand = new RelayCommand(Submit);
    }
    private void Submit()
    {
        if (!string.IsNullOrEmpty(Code))
            TwoFactorSource.SetResult(Code);
    }

    public TaskCompletionSource<string> TwoFactorSource { get; } = new();
    public ICommand SubmitCommand { get; }
    public string Code { get; set; } = string.Empty;
}
