using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Graphics.Dwm;
using Microsoft.Win32;
using MusicX.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Views;

public class MusicXWindow : FluentWindow
{
    private readonly ISnackbarService _snackbarService;
    private readonly NavigationService _navigationService;
    private ModalFrame? _frame;

    public MusicXWindow(ISnackbarService snackbarService, NavigationService navigationService)
    {
        _snackbarService = snackbarService;
        _navigationService = navigationService;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Style = (Style)FindResource("MusicXWindowStyle");
        
        var colorPrevalence = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM", false)?
            .GetValue("ColorPrevalence", 0) as int? == 1;
        
        var windowHandle = new WindowInteropHelper(this).EnsureHandle();

        if (!colorPrevalence) return;
        
        try
        {
            unsafe
            {
                var value = 0x00202020;
                var hResult = PInvoke.DwmSetWindowAttribute(new(windowHandle), DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, Unsafe.AsPointer(ref value), sizeof(int));
                Marshal.ThrowExceptionForHR(hResult);
            }
        }catch(Exception ex)
        {
            //nothing
        }
        
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        if (GetTemplateChild("PART_SnackbarPresenter") is SnackbarPresenter snackbarPresenter)
            _snackbarService.SetSnackbarPresenter(snackbarPresenter);
        else _snackbarService.SetSnackbarPresenter(null!);

        if (GetTemplateChild("PART_ModalFrame") is not ModalFrame frame) return;
        
        _frame = frame;
        _navigationService.ModalOpenRequested += NavigationServiceOnModalOpenRequested;
        _navigationService.ModalCloseRequested += NavigationServiceOnModalCloseRequested;
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