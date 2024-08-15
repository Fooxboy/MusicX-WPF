using System;
using System.ComponentModel;
using System.Windows;
using MusicX.Controls;
using MusicX.Helpers;
using MusicX.Services;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Views;

public class MusicXWindow : FluentWindow
{
    private readonly ISnackbarService _snackbarService;
    private readonly NavigationService _navigationService;
    private readonly WindowThemeService _themeService;
    private ModalFrame? _frame;

    public MusicXWindow(ISnackbarService snackbarService, NavigationService navigationService, WindowThemeService themeService)
    {
        _snackbarService = snackbarService;
        _navigationService = navigationService;
        _themeService = themeService;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Style = (Style)FindResource("MusicXWindowStyle");

        themeService.Register(this);
        this.SuppressTitleBarColorization();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            if (GetSnackbarPresenter() is { } snackbarPresenter)
                _snackbarService.SetSnackbarPresenter(snackbarPresenter);
            else _snackbarService.SetSnackbarPresenter(null!);

            if (GetTemplateChild("PART_ModalFrame") is not ModalFrame frame) return;

            _frame = frame;
            _navigationService.ModalOpenRequested += NavigationServiceOnModalOpenRequested;
            _navigationService.ModalCloseRequested += NavigationServiceOnModalCloseRequested;
        });
    }
    
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        _themeService.Unregister(this);
    }

    protected virtual SnackbarPresenter? GetSnackbarPresenter()
    {
        return GetTemplateChild("PART_SnackbarPresenter") as SnackbarPresenter;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _navigationService.ModalOpenRequested -= NavigationServiceOnModalOpenRequested;
        _navigationService.ModalCloseRequested -= NavigationServiceOnModalCloseRequested;
    }

    private void NavigationServiceOnModalCloseRequested(object? sender, EventArgs e)
    {
        _frame?.Close();
    }

    private void NavigationServiceOnModalOpenRequested(object? sender, object e)
    {
        _frame?.Open(e);
    }
}