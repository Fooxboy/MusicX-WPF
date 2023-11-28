using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Windows.UI.Popups;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MusicX.Services;
public class MusicXSnackbarService : ISnackbarService
{
    private SnackbarPresenter? _presenter;

    private Snackbar? _snackbar;

    public TimeSpan DefaultTimeOut { get; set; } = TimeSpan.FromSeconds(5.0);


    public void SetSnackbarPresenter(SnackbarPresenter? contentPresenter)
    {
        _presenter = contentPresenter;
    }

    public SnackbarPresenter GetSnackbarPresenter()
    {
        if (_presenter == null)
        {
            throw new InvalidOperationException("The SnackbarPresenter didn't set previously.");
        }

        return _presenter;
    }

    public void Show(string title, string message, ControlAppearance appearance, IconElement? icon, TimeSpan timeout)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            ShowInternal(title, message, appearance, icon, timeout);
        else
            Application.Current.Dispatcher.BeginInvoke(() => ShowInternal(title, message, appearance, icon, timeout));
    }

    private void ShowInternal(string title, string message, ControlAppearance appearance, IconElement? icon,
        TimeSpan timeout)
    {
        if (_presenter is null)
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault();
            
            if (window is null)
                return;
            
            var handle = new WindowInteropHelper(window).EnsureHandle();

            var dialog = new MessageDialog(message, title);
            
            WinRT.Interop.InitializeWithWindow.Initialize(dialog, handle);

            dialog.ShowAsync();
            return;
        }
        
        _snackbar ??= new Snackbar(_presenter);

        _snackbar!.SetCurrentValue(Snackbar.TitleProperty, title);
        _snackbar!.SetCurrentValue(ContentControl.ContentProperty, message);
        _snackbar!.SetCurrentValue(Snackbar.AppearanceProperty, appearance);
        _snackbar!.SetCurrentValue(Snackbar.IconProperty, icon);
        _snackbar!.SetCurrentValue(Snackbar.TimeoutProperty, (timeout.TotalSeconds == 0.0) ? DefaultTimeOut : timeout);
        _snackbar!.Show(immediately: true);
    }
}
