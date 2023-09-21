using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MusicX.Services;
public class MusicXSnackbarService : ISnackbarService
{
    private SnackbarPresenter? _presenter;

    private Snackbar? _snackbar;

    public TimeSpan DefaultTimeOut { get; set; } = TimeSpan.FromSeconds(5.0);


    public void SetSnackbarPresenter(SnackbarPresenter contentPresenter)
    {
        _presenter = contentPresenter;
    }

    public SnackbarPresenter GetSnackbarPresenter()
    {
        if (_presenter == null)
        {
            throw new ArgumentNullException("The SnackbarPresenter didn't set previously.");
        }

        return _presenter;
    }

    public void Show(string title, string message)
    {
        Show(title, message, ControlAppearance.Secondary, null, DefaultTimeOut);
    }

    public void Show(string title, string message, ControlAppearance appearance)
    {
        Show(title, message, appearance, null, DefaultTimeOut);
    }

    public void Show(string title, string message, IconElement icon)
    {
        Show(title, message, ControlAppearance.Secondary, icon, DefaultTimeOut);
    }

    public void Show(string title, string message, TimeSpan timeout)
    {
        Show(title, message, ControlAppearance.Secondary, null, timeout);
    }

    public void Show(string title, string message, ControlAppearance appearance, TimeSpan timeout)
    {
        Show(title, message, appearance, null, timeout);
    }

    public void Show(string title, string message, IconElement icon, TimeSpan timeout)
    {
        Show(title, message, ControlAppearance.Secondary, icon, timeout);
    }

    public void Show(string title, string message, ControlAppearance appearance, IconElement? icon, TimeSpan timeout)
    {
        if (_presenter == null)
        {
            throw new ArgumentNullException(null, "The SnackbarPresenter didn't set previously.");
        }

        if (Application.Current.Dispatcher.CheckAccess())
            ShowInternal(title, message, appearance, icon, timeout);
        else
            Application.Current.Dispatcher.BeginInvoke(() => ShowInternal(title, message, appearance, icon, timeout));
    }

    private void ShowInternal(string title, string message, ControlAppearance appearance, IconElement? icon,
        TimeSpan timeout)
    {
        _snackbar ??= new Snackbar(_presenter!);

        _snackbar!.SetCurrentValue(Snackbar.TitleProperty, title);
        _snackbar!.SetCurrentValue(ContentControl.ContentProperty, message);
        _snackbar!.SetCurrentValue(Snackbar.AppearanceProperty, appearance);
        _snackbar!.SetCurrentValue(Snackbar.IconProperty, icon);
        _snackbar!.SetCurrentValue(Snackbar.TimeoutProperty, (timeout.TotalSeconds == 0.0) ? DefaultTimeOut : timeout);
        _snackbar!.Show(immediately: true);
    }
}
