using System;
using System.Windows.Input;

namespace Wpf.Ui.Common;

/// <summary>
///     A command whose sole purpose is to relay its functionality to other objects by invoking delegates. The
///     default return value for the <see cref="CanExecute" /> method is <see langword="true" />.
///     <para>
///         <see href="https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.input.icommand?view=winrt-22000" />
///     </para>
/// </summary>
public sealed class RelayCommand : IRelayCommand
{
    private readonly Action<object?>? _execute;

    private readonly Func<bool>? _canExecute;

    /// <summary>
    ///     Event occuring when encapsulated canExecute method is changed.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void NotifyCanExecuteChanged()
    {
        _canExecute?.Invoke();
    }

    /// <summary>
    ///     Creates new instance of <see cref="RelayCommand" />.
    /// </summary>
    /// <param name="execute">Action to be executed.</param>
    /// <param name="canExecute">Encapsulated method determining whether to execute action.</param>
    /// <exception cref="ArgumentNullException">Exception occurring when no <see cref="Action" /> is defined.</exception>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        if (execute == null)
            throw new ArgumentNullException(nameof(execute));

        _execute = _ => execute();
        _canExecute = canExecute;
    }

    /// <summary>
    ///     Creates new instance of <see cref="RelayCommand" />.
    /// </summary>
    /// <param name="execute">Action with <see cref="object" /> parameter to be executed.</param>
    /// <param name="canExecute">Encapsulated method determining whether to execute action.</param>
    /// <exception cref="ArgumentNullException">Exception occurring when no <see cref="Action" /> is defined.</exception>
    public RelayCommand(Action<object?> execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <inheritdoc cref="IRelayCommand.CanExecute" />
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    /// <inheritdoc cref="IRelayCommand.Execute" />
    public void Execute(object? parameter)
    {
        _execute?.Invoke(parameter);
    }
}