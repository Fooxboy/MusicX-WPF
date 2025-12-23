using MusicX.ViewModels.Login;
using Wpf.Ui.Controls;

namespace MusicX.Views.Login;

public partial class PasswordPage : INavigableView<PasswordViewModel>
{
    public PasswordPage(PasswordViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }

    public PasswordViewModel ViewModel { get; }
}