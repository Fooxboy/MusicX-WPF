﻿using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для MusicCategoryBlockControl.xaml
    /// </summary>
    public partial class MusicCategoryBlockControl : UserControl
    {

        private readonly VkService vkService;
        private readonly Services.NavigationService navigationService;

        public MusicCategoryBlockControl()
        {
            InitializeComponent();

            vkService = StaticService.Container.GetRequiredService<VkService>();
            navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
        }

        public List<Link> Links { get; set; }

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {
            var link = Links[0];

            await OpenPage(link);

        }

        private async Task OpenPage(Link link)
        {
            var music = await vkService.GetAudioCatalogAsync(link.Url);
            navigationService.OpenSection(music.Catalog.DefaultSection);

            return;
        }

        private async void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            var link = Links[1];

            await OpenPage(link);
        }

        private async void CardAction_Click_2(object sender, RoutedEventArgs e)
        {
            var link = Links[2];

            await OpenPage(link);
        }

        private async void CardAction_Click_3(object sender, RoutedEventArgs e)
        {
            var link = Links[3];

            await OpenPage(link);
        }
    }
}
