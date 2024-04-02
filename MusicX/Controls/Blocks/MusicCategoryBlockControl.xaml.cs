using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Web;
using Microsoft.AppCenter.Crashes;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

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

        public List<Link> Links => (DataContext as Block)?.Links ?? new();

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {
            if (GetLinkBySection("recent") is { } link)
                await OpenPage(link);
        }

        private async Task OpenPage(Link link)
        {
            try
            {
                var music = await vkService.GetAudioCatalogAsync(link.Url);
                navigationService.OpenSection(music.Catalog.DefaultSection);

                return;
            }
            catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
            }

        }

        private async void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            if (GetLinkBySection("my_playlists") is { } link)
                await OpenPage(link);
        }

        private async void CardAction_Click_2(object sender, RoutedEventArgs e)
        {
            if (GetLinkBySection("albums") is { } link)
                await OpenPage(link);
        }

        private async void CardAction_Click_3(object sender, RoutedEventArgs e)
        {
            if (GetLinkBySection("followings") is { } link)
                await OpenPage(link);
        }

        private Link? GetLinkBySection(string section)
        {
            var link = Links.FirstOrDefault(b =>
                HttpUtility.ParseQueryString(new Uri(b.Url).Query)["section"]
                    ?.Equals(section, StringComparison.OrdinalIgnoreCase) is true);

            if (link is null)
                StaticService.Container.GetRequiredService<ISnackbarService>().Show("Ошибка",
                    $"Ссылка {section} не найдена", ControlAppearance.Danger);
            
            return link;
        }
    }
}
