using System.Reactive.Concurrency;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MusicX.Avalonia.ViewModels.ViewModels;
using ReactiveUI;

namespace MusicX.Avalonia.Pages;

public partial class SectionPage : ReactiveUserControl<SectionTabViewModel>
{
    public SectionPage()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (ViewModel is not null)
            RxApp.MainThreadScheduler.ScheduleAsync((_, _) => ViewModel.LoadAsync());
    }
}