using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using NLog;
using Wpf.Ui.Extensions;
namespace MusicX.Controls;

public class NavigationBar : Control
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items), typeof(ObservableCollection<NavigationBarItem>), typeof(NavigationBar));

    public ObservableCollection<NavigationBarItem> Items
    {
        get => (ObservableCollection<NavigationBarItem>)GetValue(ItemsProperty);
        init => SetValue(ItemsProperty, value);
    }

    public static readonly DependencyProperty FooterItemsProperty = DependencyProperty.Register(
        nameof(FooterItems), typeof(ObservableCollection<NavigationBarItem>), typeof(NavigationBar));

    public ObservableCollection<NavigationBarItem> FooterItems
    {
        get => (ObservableCollection<NavigationBarItem>)GetValue(FooterItemsProperty);
        set => SetValue(FooterItemsProperty, value);
    }
    
    public static readonly DependencyProperty FrameProperty = DependencyProperty.Register(
        nameof(Frame), typeof(Frame), typeof(NavigationBar), new(OnFrameChanged));

    public Frame Frame
    {
        get => (Frame)GetValue(FrameProperty);
        set => SetValue(FrameProperty, value);
    }

    public static readonly DependencyProperty CurrentItemProperty = DependencyProperty.Register(
        nameof(CurrentItem), typeof(NavigationBarItem), typeof(NavigationBar), new(OnCurrentItemChanged));

    public NavigationBarItem? CurrentItem
    {
        get => (NavigationBarItem)GetValue(CurrentItemProperty);
        set => SetValue(CurrentItemProperty, value);
    }

    public NavigationBar()
    {
        Items = new();
        FooterItems = new();
        Items.CollectionChanged += CollectionChanged;
        FooterItems.CollectionChanged += CollectionChanged;
    }
    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add when e.NewItems![0] is NavigationBarItem addedItem:
                addedItem.Click += ItemOnClick;
                break;
            case NotifyCollectionChangedAction.Remove when e.OldItems![0] is NavigationBarItem removedItem:
                removedItem.Click -= ItemOnClick;
                break;
        }
    }
    private void ItemOnClick(object sender, RoutedEventArgs e)
    {
        var item = (NavigationBarItem)sender;
        
        if (item.PageDataContext is INotifyOnActivated notifyOnActivated)
            notifyOnActivated.OnActivated();

        var page = (Page)ActivatorUtilities.CreateInstance(StaticService.Container, item.PageType);

        page.DataContext = item.PageDataContext;

        if (page is IMenuPage menuView)
            menuView.MenuTag = (string)item.Tag;
        
        Frame.Navigate(page);
    }

    private static void OnFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var bar = (NavigationBar)d;
        
        if (e.OldValue is Frame oldFrame)
            oldFrame.Navigated -= bar.FrameOnNavigated;
        
        if (e.NewValue is Frame newFrame)
            newFrame.Navigated += bar.FrameOnNavigated;
    }
    private void FrameOnNavigated(object sender, NavigationEventArgs e)
    {
        if (e.Content is IMenuPage menuView)
            CurrentItem = Items.Concat(FooterItems).FirstOrDefault(b => b.Tag == menuView.MenuTag);
        else
            CurrentItem = null;
        
        if (CurrentItem != null)
            Frame.CleanNavigation();

        Log.Info("Navigated to {Content}", e.Content.ToString());
    }
    private static void OnCurrentItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is NavigationBarItem oldItem)
            oldItem.IsSelected = false;

        if (e.NewValue is NavigationBarItem newItem)
            newItem.IsSelected = true;
    }
}

public interface INotifyOnActivated
{
    void OnActivated();
}

public interface IMenuPage
{
    string MenuTag { get; set; }
}