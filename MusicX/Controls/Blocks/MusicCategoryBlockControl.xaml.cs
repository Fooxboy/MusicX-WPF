using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            vkService = StaticService.Container.Resolve<VkService>();
            navigationService = StaticService.Container.Resolve<Services.NavigationService>();
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
            await navigationService.OpenSection(music.Catalog.DefaultSection, true);

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
