using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WPFUI.Common;
namespace MusicX.Behaviors;

public static class AutoScrollBehavior
{
    private static readonly DependencyPropertyKey ScrollControllerPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly("ScrollController", typeof(ScrollController), typeof(AutoScrollBehavior), new());
    
    public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached(
        "AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(AutoScrollChanged));
    private static void AutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var enabled = (bool)e.NewValue;
        
        if (d.GetValue(ScrollControllerPropertyKey.DependencyProperty) is ScrollController controller && !enabled)
        {
            controller.Dispose();
            d.ClearValue(ScrollControllerPropertyKey);
            return;
        }

        if (!enabled)
            return;

        var element = (ScrollViewer)d;
        d.SetValue(ScrollControllerPropertyKey, new ScrollController(element));
        element.Unloaded += TextBlockOnUnloaded;
    }
    private static void TextBlockOnUnloaded(object sender, RoutedEventArgs e)
    {
        var d = ((ScrollViewer)sender);
        d.Unloaded -= TextBlockOnUnloaded;
        
        if (d.GetValue(ScrollControllerPropertyKey.DependencyProperty) is not ScrollController controller)
            return;
        
        controller.Dispose();
        d.ClearValue(ScrollControllerPropertyKey);
    }

    public static void SetAutoScroll(DependencyObject element, bool value)
    {
        element.SetValue(AutoScrollProperty, value);
    }

    public static bool GetAutoScroll(DependencyObject element)
    {
        return (bool)element.GetValue(AutoScrollProperty);
    }

    public static readonly DependencyProperty DirectionProperty = DependencyProperty.RegisterAttached(
        "Direction", typeof(ScrollDirection), typeof(AutoScrollBehavior), new PropertyMetadata(ScrollDirection.Horizontal));

    public static void SetDirection(DependencyObject element, ScrollDirection value)
    {
        element.SetValue(DirectionProperty, value);
    }

    public static ScrollDirection GetDirection(DependencyObject element)
    {
        return (ScrollDirection)element.GetValue(DirectionProperty);
    }

    public static ScrollController? GetController(DependencyObject element)
    {
        return (ScrollController?)element.GetValue(ScrollControllerPropertyKey.DependencyProperty);
    }
}

public class ScrollController : IDisposable
{
    private readonly ScrollViewer _target;
    private readonly DispatcherTimer _timer;
    
    private bool _backScroll;
    public ScrollController(ScrollViewer target)
    {
        _target = target;
        _timer = new(TimeSpan.FromMilliseconds(30), DispatcherPriority.Background, Callback, Application.Current.Dispatcher);
        _timer.Start();
    }
    private void Callback(object? sender, EventArgs e)
    {
        var isHorizontal = AutoScrollBehavior.GetDirection(_target) == ScrollDirection.Horizontal;
        
        if ((isHorizontal ? _target.ScrollableWidth : _target.ScrollableHeight) == 0) return;

        if (!_backScroll)
        {
            _target.ScrollToHorizontalOffset((isHorizontal ? _target.HorizontalOffset : _target.VerticalOffset) + 0.6);
            if (Math.Abs((isHorizontal ? _target.HorizontalOffset : _target.VerticalOffset) - (isHorizontal ? _target.ScrollableWidth : _target.ScrollableHeight)) < 0.01)
                _backScroll = true;
        }

        if (_backScroll)
        {
            _target.ScrollToHorizontalOffset((isHorizontal ? _target.HorizontalOffset : _target.VerticalOffset) - 0.8);
            if ((isHorizontal ? _target.HorizontalOffset : _target.VerticalOffset) < 0.01)
                _backScroll = false;
        }
    }

    public void Pause()
    {
        if (_timer.IsEnabled)
            _timer.Stop();
    }

    public void Play()
    {
        if (!_timer.IsEnabled)
            _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
