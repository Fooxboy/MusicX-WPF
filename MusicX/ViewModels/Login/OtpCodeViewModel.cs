using System.Threading.Tasks;
using System.Windows.Input;
using VkNet.Extensions.Auth.Models.Auth;
using Wpf.Ui.Input;

namespace MusicX.ViewModels.Login;

public class OtpCodeViewModel : BaseViewModel
{
    public int CodeLength { get; set; }
    public string? Info { get; set; }
    public LoginWay LoginWay { get; set; } = LoginWay.None;
    public bool HasAnotherVerificationMethods { get; set; }
    
    public ICommand ShowAnotherVerificationMethodsCommand { get; }
    public ICommand SubmitCommand { get; }
    public TaskCompletionSource<string?> Submitted { get; private set; } = new();

    public OtpCodeViewModel()
    {
        SubmitCommand = new RelayCommand<string?>(code =>
        {
            Submitted.SetResult(code);
            Submitted = new();
        });
        ShowAnotherVerificationMethodsCommand = new RelayCommand<string>(_ =>
        {
            Submitted.SetResult(null);
            Submitted = new();
        });
    }
}