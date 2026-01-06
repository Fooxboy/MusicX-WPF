using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Windows.Foundation;
using Windows.UI.Popups;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MusicX.Services;

public sealed class MusicXSnackbarService : ISnackbarService, IAsyncDisposable
{
    private SnackbarPresenter? _presenter;

    private Snackbar? _snackbar;
    private IAsyncOperation<IUICommand>? _activeDialog;

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

            _activeDialog = dialog.ShowAsync();
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

    public async ValueTask DisposeAsync()
    {
        if (_activeDialog is not null)
            await _activeDialog;
    }
}
