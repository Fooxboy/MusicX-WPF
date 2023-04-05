using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MusicX.Avalonia.Pages;

public partial class LoginPage : UserControl
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}