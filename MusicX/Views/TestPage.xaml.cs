using System;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Windows.UI.Popups;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Controls;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.ViewModels;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using NLog;
using VkNet.Abstractions.Core;
using VkNet.Exception;
using WinRT.Interop;
using Wpf.Ui;
using Wpf.Ui.Extensions;
using NavigationService = MusicX.Services.NavigationService;

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

            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();

            navigationService.OpenModal<TestModal>();
        }

        private void OpenSectionButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            navigationService.OpenSection(section.Text);
        }

        private void openArtist_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            navigationService.OpenSection(artist.Text, SectionType.Artist);
        }

        private void showNotification_Click(object sender, RoutedEventArgs e)
        {
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

            snackbarService.Show("Заголовок", "Сообщение", TimeSpan.FromSeconds(10));

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
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            var viewModel = StaticService.Container.GetRequiredService<PlaylistSelectorModalViewModel>();

            navigationService.OpenModal<PlaylistSelectorModal>(viewModel);
        }

        private void OpenPlaylistModal_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
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

                var brr = new MessageDialog($"{ex.Message} \n \n \n {ex.StackTrace}", "Ошибка");
                InitializeWithWindow.Initialize(brr, hwnd);

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

                var brr = new MessageDialog($"{ex.Message} \n \n \n {ex.StackTrace}", "Ошибка");
                InitializeWithWindow.Initialize(brr, hwnd);

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

        private void RaiseCaptcha_OnClick(object sender, RoutedEventArgs e)
        {
            var handler = StaticService.Container.GetRequiredService<ICaptchaHandler>();

            handler.Perform(async (sid, key) =>
            {
                if (sid.HasValue)
                {
                    var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
                    snackbarService.Show("Капча", $"Вы ввели '{key}'");
                    return 0;
                }
                
                const ulong captchaSid = 123456;
                throw new CaptchaNeededException(new()
                {
                    CaptchaImg = new($"https://api.vk.com//captcha.php?sid={captchaSid}&s=1"),
                    CaptchaSid = captchaSid
                });
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new Exception("Test");
            }
            catch (Exception exception)
            {
                StaticService.Container.GetRequiredService<Logger>().Fatal(exception, "Test");
            }
        }

        private void PlaylistSerialize_OnClick(object sender, RoutedEventArgs e)
        {
            var playerState = PlayerState.CreateOrNull(StaticService.Container.GetRequiredService<PlayerService>());
            
            var json = JsonSerializer.Serialize(playerState);

            bool valid;
            try
            {
                var playlist = JsonSerializer.Deserialize<Playlist>(json);
                valid = playlist is not null;
            }
            catch (JsonException)
            {
                valid = false;
            }
            
            StaticService.Container.GetRequiredService<ISnackbarService>().Show($"Valid: {valid}", json);
            Debug.WriteLine(json);
            Debug.WriteLine($"valid: {valid}");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();

            navigationService.OpenModal<WelcomeToListenTogetherModal>();
        }
    }
}
