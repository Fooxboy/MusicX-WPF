using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using MusicX.Avalonia.ViewModels.ViewModels;

namespace MusicX.Avalonia.Pages;

public partial class PlaylistPage : ReactiveUserControl<PlaylistViewModel>
{
    public PlaylistPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}