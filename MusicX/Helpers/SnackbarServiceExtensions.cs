using System;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MusicX.Helpers;

public static class SnackbarServiceExtensions
{
    public static void ShowException(this ISnackbarService snackbarService, string title, Exception exception)
    {
        ShowException(snackbarService, title, exception.Message);
    }
    
    public static void ShowException(this ISnackbarService snackbarService, string title, string message)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            snackbarService.Show(title, message, ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24), snackbarService.DefaultTimeOut);
        else
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                snackbarService.Show(title, message, ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), snackbarService.DefaultTimeOut);
            });
    }
}