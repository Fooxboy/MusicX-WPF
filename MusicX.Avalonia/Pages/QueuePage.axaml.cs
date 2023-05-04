using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DynamicData.Binding;
using MusicX.Avalonia.ViewModels.ViewModels;
using ReactiveUI;

namespace MusicX.Avalonia.Pages;

public partial class QueuePage : ReactiveUserControl<QueueViewModel>
{
    public QueuePage()
    {
        InitializeComponent();
    }
    
    // https://github.com/AvaloniaUI/Avalonia/issues/8684
    public static readonly DirectProperty<QueuePage, QueueViewModel> ViewModelDirectProperty =
        AvaloniaProperty.RegisterDirect<QueuePage, QueueViewModel>(
            "ViewModelDirect", o => o.ViewModelDirect, (o, v) => o.ViewModelDirect = v);

    public QueueViewModel ViewModelDirect
    {
        get => ViewModel!;
        set
        {
            var viewModel = ViewModel!;
            ViewModel = value;
            RaisePropertyChanged(ViewModelDirectProperty, viewModel, value);
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ViewModelDirect.PlayerService.WhenValueChanged(x => x.CurrentTrack)
                       .ObserveOn(RxApp.MainThreadScheduler)
                       .Where(b => b is not null)
                       .Select(b => Queue.ContainerFromItem(b!))
                       .Subscribe(b => b?.BringIntoView());
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        if (ViewModelDirect.PlayerService.CurrentTrack is not null)
            Queue.ContainerFromItem(ViewModelDirect.PlayerService.CurrentTrack)?.BringIntoView();
    }
}