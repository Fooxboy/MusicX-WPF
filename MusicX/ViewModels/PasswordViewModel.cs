using System.Windows.Input;
using VkNet.AudioBypassService.Models.Vk;
using Wpf.Ui.Common;
namespace MusicX.ViewModels;

public class PasswordViewModel : BaseViewModel
{
    private readonly LoginViewModel _viewModel;
    public PasswordViewModel(ValidatePhoneProfile profile, LoginViewModel viewModel)
    {
        _viewModel = viewModel;
        Profile = profile;
        SubmitCommand = new RelayCommand(Submit);
    }
    private void Submit()
    {
        if (Password.Length > 8)
            _viewModel.PasswordSource.SetResult(Password);
    }

    public ICommand SubmitCommand { get; }
    public ValidatePhoneProfile Profile { get; }
    public string ButtonContent => $"Войти как {Profile.FirstName} {Profile.LastName}";
    public string Password { get; set; } = string.Empty;
}
