using DryIoc;
using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Views.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicX.Controls;
using MusicX.ViewModels;
using MusicX.ViewModels.Modals;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для TestPage.xaml
    /// </summary>
    public partial class TestPage : Page, IMenuPage
    {
        public TestPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //new PlayerService().Play();
        }

        private async void OpenModal_Click(object sender, RoutedEventArgs e)
        {

            /*var window = Application.Current.MainWindow;
            // ...
            IntPtr hwnd = new WindowInteropHelper(window).Handle;



            var brr =new Windows.UI.Popups.MessageDialog("brrrrrrrr", "brrrrr");
            WinRT.Interop.InitializeWithWindow.Initialize(brr, hwnd);

            await brr.ShowAsync();*/

            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

            navigationService.OpenModal<TestModal>();
        }

        private void OpenSectionButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
            navigationService.OpenSection(section.Text);
        }

        private void openArtist_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
            navigationService.OpenSection(artist.Text, SectionType.Artist);
        }

        private void showNotification_Click(object sender, RoutedEventArgs e)
        {
            var notificationsService = StaticService.Container.Resolve<Services.NotificationsService>();

            notificationsService.Show("Заголовок", "Сообщение");

        }

        private void download_Click(object sender, RoutedEventArgs e)
        {
            var downloader = StaticService.Container.Resolve<DownloaderViewModel>();

            var audio = new Audio()
            {
                Artist = "artist name",
                Title = "track name",
                Url = url.Text
            };
            
            downloader.DownloadQueue.Add(audio);
        }   

        private void OpenPlaylistSelector_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
            var viewModel = StaticService.Container.Resolve<PlaylistSelectorModalViewModel>();

            navigationService.OpenModal<PlaylistSelectorModal>(viewModel);
        }

        private void OpenPlaylistModal_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
            var viewModel = StaticService.Container.Resolve<CreatePlaylistModalViewModel>();

            navigationService.OpenModal<CreatePlaylistModal>(viewModel);
        }
        public string MenuTag { get; set; }
    }
}
