using System;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Common;
using Button = System.Windows.Controls.Button;
namespace MusicX.Controls;

public class NavigationBarItem : Button
{
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected), typeof(bool), typeof(NavigationBarItem));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(SymbolRegular), typeof(NavigationBarItem));

    public SymbolRegular Icon
    {
        get => (SymbolRegular)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty PageTypeProperty = DependencyProperty.Register(
        nameof(PageType), typeof(Type), typeof(NavigationBarItem));

    public Type PageType
    {
        get => (Type)GetValue(PageTypeProperty);
        set => SetValue(PageTypeProperty, value);
    }

    public static readonly DependencyProperty PageDataContextProperty = DependencyProperty.Register(
        nameof(PageDataContext), typeof(object), typeof(NavigationBarItem));

    public object? PageDataContext
    {
        get => (object?)GetValue(PageDataContextProperty);
        set => SetValue(PageDataContextProperty, value);
    }

    public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(
        nameof(IconForeground), typeof(Brush), typeof(NavigationBarItem));

    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }
}
