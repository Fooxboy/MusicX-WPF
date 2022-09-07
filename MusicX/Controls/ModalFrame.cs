using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Common;
namespace MusicX.Controls;

public class ModalFrame : Control
{
    public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent(
        nameof(Opened),
        RoutingStrategy.Direct,
        typeof(RoutedEventHandler),
        typeof(ModalFrame));

    public event RoutedEventHandler Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }
    
    public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
        nameof(Closed),
        RoutingStrategy.Direct,
        typeof(RoutedEventHandler),
        typeof(ModalFrame));

    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public static readonly DependencyProperty ModalContentProperty = DependencyProperty.Register(
        nameof(ModalContent), typeof(Page), typeof(ModalFrame));

    public Page? ModalContent
    {
        get => (Page?)GetValue(ModalContentProperty);
        set => SetValue(ModalContentProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(ModalFrame));

    public string? Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
        nameof(CloseCommand), typeof(ICommand), typeof(ModalFrame));

    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        init => SetValue(CloseCommandProperty, value);
    }

    public ModalFrame()
    {
        KeyUp += OnKeyUp;
        CloseCommand = new RelayCommand(Close);
    }
    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (ModalContent is not null && e.Key == Key.Escape)
            Close();
    }

    public void Open(object content)
    {
        if (ModalContent is not null)
            Close();

        ModalContent = (Page)content;
        
        if (ModalContent is ModalPage modalPage)
            modalPage.Closed += ModalPageOnClosed;
        
        RaiseEvent(new(OpenedEvent));
    }
    
    private void ModalPageOnClosed(object sender, RoutedEventArgs e)
    {
        ((ModalPage)sender).Closed -= ModalPageOnClosed;
        Close();
    }

    public void Close()
    {
        ModalContent = null;
        Title = null;
        RaiseEvent(new(ClosedEvent));
    }
}
