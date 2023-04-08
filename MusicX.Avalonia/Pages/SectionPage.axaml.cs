using System.Reactive.Concurrency;
using Avalonia.Controls;
using MusicX.Avalonia.ViewModels.ViewModels;
using ReactiveUI;

namespace MusicX.Avalonia.Pages;

public partial class SectionPage : UserControl
{
    public SectionPage()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is SectionTabViewModel viewModel)
            RxApp.MainThreadScheduler.ScheduleAsync((_, _) => viewModel.LoadAsync());
    }
}