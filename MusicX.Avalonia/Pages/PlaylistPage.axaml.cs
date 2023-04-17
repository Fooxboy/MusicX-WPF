using Avalonia;
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
    
    // https://github.com/AvaloniaUI/Avalonia/issues/8684
    public static readonly DirectProperty<PlaylistPage, PlaylistViewModel> ViewModelDirectProperty =
        AvaloniaProperty.RegisterDirect<PlaylistPage, PlaylistViewModel>(
            "ViewModelDirect", o => o.ViewModelDirect, (o, v) => o.ViewModelDirect = v);

    public PlaylistViewModel ViewModelDirect
    {
        get => ViewModel!;
        set
        {
            var viewModel = ViewModel!;
            ViewModel = value;
            RaisePropertyChanged(ViewModelDirectProperty, viewModel, value);
        }
    }
}