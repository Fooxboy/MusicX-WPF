using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MusicX.Avalonia.ViewModels.ViewModels;

namespace MusicX.Avalonia.Pages;

public partial class LoginPage : ReactiveUserControl<LoginModalViewModel>
{
    public LoginPage()
    {
        InitializeComponent();
    }
}