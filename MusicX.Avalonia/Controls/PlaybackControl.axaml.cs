using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MusicX.Avalonia.ViewModels.ViewModels;

namespace MusicX.Avalonia.Controls;

public partial class PlaybackControl : ReactiveUserControl<PlaybackViewModel>
{
    public PlaybackControl()
    {
        InitializeComponent();
        ViewModel = App.Provider.GetService<PlaybackViewModel>();
    }
}