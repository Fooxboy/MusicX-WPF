using System.Windows;
using System.Windows.Controls;
namespace MusicX.Controls;

public class ModalPage : Page
{
    public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
        nameof(Closed),
        RoutingStrategy.Direct,
        typeof(RoutedEventHandler),
        typeof(ModalPage));
    
    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public void Close()
    {
        RaiseEvent(new(ClosedEvent));
    }
}
