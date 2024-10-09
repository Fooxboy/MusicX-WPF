using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Media;
using MusicX.ViewModels;
using NLog;
using Wpf.Ui.Appearance;

namespace MusicX.Controls.Blocks;

/// <summary>
/// Логика взаимодействия для MusicCategoryBlockControl.xaml
/// </summary>
public partial class MusicCategoryBlockControl : UserControl
{

    private readonly VkService vkService;
    private readonly Services.NavigationService navigationService;

    public static readonly DependencyProperty AppThemeProperty =
        DependencyProperty.Register(nameof(AppTheme), typeof(ApplicationTheme), typeof(MusicCategoryBlockControl));

    public ApplicationTheme AppTheme
    {
        get => (ApplicationTheme)GetValue(AppThemeProperty);
        set => SetValue(AppThemeProperty, value);
    }

    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(
        nameof(Layout), typeof(MusicCategoryLayout), typeof(MusicCategoryBlockControl), new PropertyMetadata(MusicCategoryLayout.List));

    public MusicCategoryLayout Layout
    {
        get => (MusicCategoryLayout)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public MusicCategoryBlockControl()
    {
        InitializeComponent();
        Unloaded += OnUnloaded;

        AppTheme = ApplicationThemeManager.GetAppTheme();
        ApplicationThemeManager.Changed += ApplicationThemeOnChanged;

        vkService = StaticService.Container.GetRequiredService<VkService>();
        navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Changed -= ApplicationThemeOnChanged;
    }

    private void ApplicationThemeOnChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        AppTheme = currentApplicationTheme;
    }
        
    public IList<Link> Links => (DataContext as BlockViewModel)?.Links ?? new();

    private async Task OpenPage(Link link)
    {
        try
        {
            if (link.Meta?.ContentType is "custom")
            {
                navigationService.OpenSection(link.Meta.TrackCode);
                return;
            }
                
            var music = await vkService.GetAudioCatalogAsync(link.Url);
            navigationService.OpenSection(music.Catalog.DefaultSection);

            return;
        }
        catch(Exception ex)
        {
            var logger = StaticService.Container.GetRequiredService<Logger>();
            logger.Error(ex, "Failed to open link {LinkType} {Link}", link.Meta?.ContentType, link.Url);
        }

    }

    private async void CardAction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: Link link })
            await OpenPage(link);
    }
}

public enum MusicCategoryLayout
{
    List,
    Grid
}