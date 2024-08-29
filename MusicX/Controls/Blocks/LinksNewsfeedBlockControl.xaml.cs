using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;

namespace MusicX.Controls.Blocks;

public partial class LinksNewsfeedBlockControl : UserControl
{
    public LinksNewsfeedBlockControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LinksProperty = DependencyProperty.Register(
        nameof(Links), typeof(IEnumerable<Link>), typeof(LinksNewsfeedBlockControl));

    public IEnumerable<Link> Links
    {
        get => (IEnumerable<Link>)GetValue(LinksProperty);
        set => SetValue(LinksProperty, value);
    }
    private async void Link_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement {DataContext: Link link})
            return;
        
        var vkService = StaticService.Container.GetRequiredService<VkService>();
        var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
        var logger = StaticService.Container.GetRequiredService<Logger>();
        
        try
        {
            switch (link.Meta.ContentType)
            {
                case null:
                {
                    var match = Regex.Match(link.Url, "https://vk.com/podcasts\\?category=[0-9]+$");

                    if (match.Success)
                    {
                        //var podcasts = await vkService.GetPodcastsAsync(Link.Url);
                        //await navigationService.OpenSection(podcasts.Catalog.DefaultSection, true);

                        return;

                    }
                    var music = await vkService.GetAudioCatalogAsync(link.Url);
                    navigationService.OpenSection(music.Catalog.DefaultSection);

                    return;
                }
                case "artist":
                {
                    var url = new Uri(link.Url);

                    navigationService.OpenSection(url.Segments.LastOrDefault(), SectionType.Artist);
                    break;
                }
                case "group" or "user":
                {
                    var match = Regex.Match(link.Url, "https://vk.com/audios[0-9]+$");
                    if(match.Success)
                    {
                        var music = await vkService.GetAudioCatalogAsync(link.Url);

                        navigationService.OpenSection(music.Catalog.DefaultSection);

                        return;
                    }

                  
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link.Url,
                        UseShellExecute = true
                    });
                    break;
                }
                case "curator":
                {
                    var curator = await vkService.GetAudioCuratorAsync(link.Meta.TrackCode, link.Url);

                    navigationService.OpenSection(curator.Catalog.DefaultSection);
                    break;
                }
            }

        }
        catch(Exception ex)
        {
            logger.Error(ex, "Failed click action in link control {LinkType} {Link}", link.Meta?.ContentType, link.Url);
        }
    }
}
