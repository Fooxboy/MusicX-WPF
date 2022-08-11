using System.Threading.Tasks;
using System.Windows.Input;
using VkNet.AudioBypassService.Models.Vk;
using Wpf.Ui.Common;
namespace MusicX.ViewModels.Login;

public class PasswordViewModel : BaseViewModel
{
    public PasswordViewModel(ValidatePhoneProfile? profile)
    {
        Profile = profile;
        SubmitCommand = new RelayCommand(Submit);
    }
    private void Submit()
    {
        if (Password.Length > 8)
            PasswordSource.SetResult(Password);
    }

    public TaskCompletionSource<string> PasswordSource { get; } = new();
    public ICommand SubmitCommand { get; }
    public ValidatePhoneProfile? Profile { get; }
    public string ButtonContent => Profile is null ? "Войти" : $"Войти как {Profile.FirstName} {Profile.LastName}";
    public string Password { get; set; } = string.Empty;
}
