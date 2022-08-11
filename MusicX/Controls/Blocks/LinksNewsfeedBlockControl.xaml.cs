﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.ViewModels;
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
        
        var vkService = StaticService.Container.Resolve<VkService>();
        var navigationService = StaticService.Container.Resolve<NavigationService>();
        var logger = StaticService.Container.Resolve<Logger>();
        
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
                    await navigationService.OpenSection(music.Catalog.DefaultSection, true);

                    return;
                }
                case "artist":
                {
                    var url = new Uri(link.Url);

                    await navigationService.OpenArtistSection(url.Segments.LastOrDefault());
                    break;
                }
                case "group" or "user":
                {
                    var match = Regex.Match(link.Url, "https://vk.com/audios[0-9]+$");
                    if(match.Success)
                    {
                        var music = await vkService.GetAudioCatalogAsync(link.Url);

                        await navigationService.OpenSection(music.Catalog.DefaultSection);

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

                    await navigationService.OpenSection(curator.Catalog.DefaultSection);
                    break;
                }
            }

        }catch(Exception ex)
        {
            logger.Error("Fail click action in link control");
            logger.Error(ex, ex.Message);
        }
    }
}