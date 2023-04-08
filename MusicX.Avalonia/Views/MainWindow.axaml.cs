using System.Reactive.Linq;
using DynamicData.Binding;
using FluentAvalonia.UI.Windowing;
using MusicX.Avalonia.ViewModels.ViewModels;
using ReactiveUI;

namespace MusicX.Avalonia.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.WhenValueChanged(x => x.CurrentTab)
                     .ObserveOn(RxApp.MainThreadScheduler)
                     .Where(b => b is not null)
                     .Subscribe(tab => FrameView.NavigateFromObject(tab));
            viewModel.WhenValueChanged(x => x.CurrentModal)
                     .ObserveOn(RxApp.MainThreadScheduler)
                     .Where(b => b is not null)
                     .Subscribe(modal => ModalFrame.NavigateFromObject(modal));
            viewModel.WhenValueChanged(x => x.CurrentModal)
                     .ObserveOn(RxApp.MainThreadScheduler)
                     .Select(b => b is not null)
                     .BindTo(ModalFrame, x => x.IsVisible);
            viewModel.WhenValueChanged(x => x.CurrentModal)
                     .ObserveOn(RxApp.MainThreadScheduler)
                     .Select(b => b is null)
                     .BindTo(NavGrid, x => x.IsHitTestVisible);
        }
    }
}