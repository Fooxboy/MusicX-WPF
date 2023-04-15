using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MusicX.Avalonia.Pages;

public partial class VideoPage : UserControl
{
    public VideoPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}