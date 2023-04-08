using Avalonia.Controls;
using MusicX.Avalonia.ViewModels.ViewModels;

namespace MusicX.Avalonia.Controls;

public partial class PlaybackControl : UserControl
{
    public PlaybackControl()
    {
        InitializeComponent();
        DataContext = App.Provider.GetService<PlaybackViewModel>();
    }
}