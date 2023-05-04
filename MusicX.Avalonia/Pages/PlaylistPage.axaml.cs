using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using MusicX.Avalonia.ViewModels.ViewModels;
using ReactiveUI;

namespace MusicX.Avalonia.Pages;

public partial class PlaylistPage : ReactiveUserControl<PlaylistViewModel>
{
    public PlaylistPage()
    {
        InitializeComponent();
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
    
    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var scrollableHeight = Math.Max(0.0, MainScrollViewer.Extent.Height - MainScrollViewer.Viewport.Height);
        if (ViewModel!.IsLoading || Math.Abs(MainScrollViewer.Offset.Y - scrollableHeight) is > 200 or < 1)
            return;

        RxApp.MainThreadScheduler.ScheduleAsync((_, _) => ViewModel.LoadMoreAsync());
    }
}