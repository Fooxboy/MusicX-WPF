using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Views.Modals;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Controls;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.ViewModels;
using MusicX.ViewModels.Modals;
using System;
using System.Windows.Interop;
using MusicX.Core.Services;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для TestPage.xaml
    /// </summary>
    public partial class TestPage : Page, IMenuPage
    {
        public TestPage()
        {
            this.Loaded += TestPage_Loaded;
            InitializeComponent();
        }

        private void TestPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Lyrics.SetLines(new System.Collections.Generic.List<string>()
            //{
            //    "Первая стровка текста",
            //    "Вторая стровка текста",
            //    "Третья стровка текста",
            //    "Четвертая стровка текста",
            //    "Пятая стровка текста",
            //    "Шестая стровка текста",
            //    "Седьмая стровка текста",
            //    "Восьмая стровка текста",
            //});

            //var timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(2);
            //timer.Tick += Timer_Tick;
            //timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //Lyrics.NextLine();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //new PlayerService().Play();
        }

        private async void OpenModal_Click(object sender, RoutedEventArgs e)
        {

            /*var window = Window.GetWindow(this);
            // ...
            IntPtr hwnd = new WindowInteropHelper(window).Handle;



            var brr =new Windows.UI.Popups.MessageDialog("brrrrrrrr", "brrrrr");
            WinRT.Interop.InitializeWithWindow.Initialize(brr, hwnd);

            await brr.ShowAsync();*/

            var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

            navigationService.OpenModal<TestModal>();
        }

        private void OpenSectionButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
            navigationService.OpenSection(section.Text);
        }

        private void openArtist_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
            navigationService.OpenSection(artist.Text, SectionType.Artist);
        }

        private void showNotification_Click(object sender, RoutedEventArgs e)
        {
            var notificationsService = StaticService.Container.GetRequiredService<Services.NotificationsService>();

            notificationsService.Show("Заголовок", "Сообщение");

        }

        private void download_Click(object sender, RoutedEventArgs e)
        {
            var downloader = StaticService.Container.GetRequiredService<DownloaderViewModel>();

            var audio = new Audio()
            {
                Artist = "artist name",
                Title = "track name",
                Url = url.Text
            };
            
            downloader.DownloadQueue.Add(audio.ToTrack());
        }   

        private void OpenPlaylistSelector_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
            var viewModel = StaticService.Container.GetRequiredService<PlaylistSelectorModalViewModel>();

            navigationService.OpenModal<PlaylistSelectorModal>(viewModel);
        }

        private void OpenPlaylistModal_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
            var viewModel = StaticService.Container.GetRequiredService<CreatePlaylistModalViewModel>();

            navigationService.OpenModal<CreatePlaylistModal>(viewModel);
        }
        public string MenuTag { get; set; }

        private async void ListenTogether_OnClick(object sender, RoutedEventArgs e)
        {
            var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();
            var configService = StaticService.Container.GetRequiredService<ConfigService>();
            var config = await configService.GetConfig();


            await listenTogetherService.ConnectToServerAsync(config.UserId);
            await listenTogetherService.JoinToSesstionAsync(UserId.Text);
        }

        private async void playTogetherSessionStart_Click(object sender, RoutedEventArgs e)
        {
            var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();
            var configService = StaticService.Container.GetRequiredService<ConfigService>();

            var config = await configService.GetConfig();

            await listenTogetherService.StartSessionAsync(config.UserId);
        }

        private async void playTogetherSessionStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();

                await listenTogetherService.StopPlaySessionAsync();
            }catch(Exception ex)
            {
                var window = Window.GetWindow(this);

                IntPtr hwnd = new WindowInteropHelper(window).Handle;

                var brr = new Windows.UI.Popups.MessageDialog($"{ex.Message} \n \n \n {ex.StackTrace}", "Ошибка");
                WinRT.Interop.InitializeWithWindow.Initialize(brr, hwnd);

                await brr.ShowAsync();
            }
        }

        private async void DisconnectListenTogether_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();

                await listenTogetherService.LeavePlaySessionAsync();
            }
            catch (Exception ex)
            {
                var window = Window.GetWindow(this);

                IntPtr hwnd = new WindowInteropHelper(window).Handle;

                var brr = new Windows.UI.Popups.MessageDialog($"{ex.Message} \n \n \n {ex.StackTrace}", "Ошибка");
                WinRT.Interop.InitializeWithWindow.Initialize(brr, hwnd);

                await brr.ShowAsync();
            }
        }

        private void Mixer_Click(object sender, RoutedEventArgs e)
        {
            var value = float.Parse(ValueMixer.Text);
            new WindowsAudioMixerService().SetVolume(value);
        }

        private void MixerGet_Click(object sender, RoutedEventArgs e)
        {
            var value = new WindowsAudioMixerService().GetVolume();

            CurrentMixer.Text = $"Текущее значение: {value}";
        }
    }
}
