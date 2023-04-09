﻿using System.Reactive.Concurrency;
using Avalonia;
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
    
    // https://github.com/AvaloniaUI/Avalonia/issues/8684
    public static readonly DirectProperty<SectionPage, SectionTabViewModel> ViewModelDirectProperty = AvaloniaProperty.RegisterDirect<SectionPage, SectionTabViewModel>(
        "ViewModelDirect", o => o.ViewModelDirect, (o, v) => o.ViewModelDirect = v);

    public SectionTabViewModel ViewModelDirect
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
        if (ViewModel is not null)
            RxApp.MainThreadScheduler.ScheduleAsync((_, _) => ViewModel.LoadAsync());
    }
}