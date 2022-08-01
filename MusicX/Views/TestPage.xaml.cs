using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views.Modals;
using System.Diagnostics;

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

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для TestPage.xaml
    /// </summary>
    public partial class TestPage : Page
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

            var window = Application.Current.MainWindow;
            // ...
            IntPtr hwnd = new WindowInteropHelper(window).Handle;



            var brr =new Windows.UI.Popups.MessageDialog("brrrrrrrr", "brrrrr");
            WinRT.Interop.InitializeWithWindow.Initialize(brr, hwnd);

            await brr.ShowAsync();

            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

            //navigationService.OpenModal(new TestModal(), 340, 500);
        }

        private async void OpenSectionButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

            navigationService.CurrentFrame.Navigate(navigationService.SectionView);
            await navigationService.OpenSection(section.Text);
        }

        private async void openArtist_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

            navigationService.CurrentFrame.Navigate(navigationService.SectionView);

            await navigationService.OpenArtistSection(artist.Text);
        }

        private void showNotification_Click(object sender, RoutedEventArgs e)
        {
            var notificationsService = StaticService.Container.Resolve<Services.NotificationsService>();

            notificationsService.Show("Заголовок", "Сообщение");

        }

        private async void download_Click(object sender, RoutedEventArgs e)
        {
            var downloader = StaticService.Container.Resolve<Services.DownloaderService>();

            var audio = new Audio()
            {
                Artist = "artist name",
                Title = "track name",
                Url = url.Text
            };

            await downloader.AddToQueueAsync(audio);

        }

        private async void OpenLastFMAuth_Click(object sender, RoutedEventArgs e)
        {
            var lastFmService = StaticService.Container.Resolve<LastFMService>();

            var url = lastFmService.GetUrlAuth();

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });

        }

        private async void ready_Click(object sender, RoutedEventArgs e)
        {
            var lastFmService = StaticService.Container.Resolve<LastFMService>();

            await lastFmService.ScrobbleAsync(new Audio() { Title = "Мы смогли", Artist = "Johnyboy", Duration = 140 });
        }
    }
}
