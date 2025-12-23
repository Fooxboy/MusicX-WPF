using System.Threading.Tasks;
using System.Windows.Input;
using VkNet.Extensions.Auth.Models.Ecosystem;
using Wpf.Ui.Input;

namespace MusicX.ViewModels.Login;

public class PasswordViewModel : BaseViewModel
{
    public EcosystemProfile? Profile { get; set; }
    
    public ICommand PasswordSubmitCommand { get; }
    
    public TaskCompletionSource<string?> PasswordSubmitted { get; private set; } = new();

    public PasswordViewModel()
    {
        PasswordSubmitCommand = new RelayCommand<string>(password =>
        {
            PasswordSubmitted.SetResult(password);
            PasswordSubmitted = new();
        });
    }
}