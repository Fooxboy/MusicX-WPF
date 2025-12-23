using MusicX.ViewModels.Login;
using Wpf.Ui.Controls;

namespace MusicX.Views.Login;

public partial class LoginPage : INavigableView<LoginViewModel>
{
    public LoginPage(LoginViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }

    public LoginViewModel ViewModel { get; }
}