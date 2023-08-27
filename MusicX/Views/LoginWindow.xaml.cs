using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using NLog;
using VkNet.AudioBypassService.Exceptions;
using Wpf.Ui;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        private readonly VkService vkService;
        private readonly ConfigService configService;
        private readonly Logger logger;
        private readonly NavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;

        private readonly bool tokenRefresh;

        public LoginWindow(VkService vkService, ConfigService configService, Logger logger,
            NavigationService navigationService, ISnackbarService snackbarService, bool tokenRefresh = false)
        {
            InitializeComponent();
            this.vkService = vkService;
            this.configService = configService;
            this.logger = logger;
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            this.tokenRefresh = tokenRefresh;
            WpfTitleBar.MaximizeClicked += WpfTitleBar_MaximizeClicked;

            if(Environment.OSVersion.Version.Build < WindowsBuild.Windows10_1607)
            {
                UnsupportOsBlock.Visibility = Visibility.Visible;
            }
            
            navigationService.ModalOpenRequested += NavigationServiceOnModalOpenRequested;
            navigationService.ModalCloseRequested += NavigationServiceOnModalCloseRequested;
            snackbarService.SetSnackbarPresenter(RootSnackbar);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _navigationService.ModalOpenRequested -= NavigationServiceOnModalOpenRequested;
            _navigationService.ModalCloseRequested -= NavigationServiceOnModalCloseRequested;
        }

        private void NavigationServiceOnModalCloseRequested(object? sender, EventArgs e)
        {
            ModalFrame.Close();
        }
        private void NavigationServiceOnModalOpenRequested(object? sender, object e)
        {
            ModalFrame.Open(e);
        }

        private bool isFullScreen = false;

        private void WpfTitleBar_MaximizeClicked(object sender, RoutedEventArgs e)
        {
            isFullScreen = !isFullScreen;
            if (isFullScreen)
            {
                rootGrid.Margin = new Thickness(8, 8, 8, 0);

            }
            else
            {
                rootGrid.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var os = Environment.OSVersion;

            logger.Info($"OS Version: {os.VersionString}");
            logger.Info($"OS Build: {os.Version.Build}");
           

            logger.Info("Loaded Login window");

            if(tokenRefresh)
            {
                _snackbarService.Show("Токен устарел", "Войдите в аккаунт снова, чтобы продолжить пользоваться MusicX");
            }
        }

        public bool CodeAccepted = false;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;
                content.Visibility = Visibility.Collapsed;

                var login = Login.Text;
                var password = Password.Password;

                var token = await vkService.AuthAsync(login, password, TwoAuthDelegate);
                var currentUser = await vkService.GetCurrentUserAsync();

                var config = await configService.GetConfig();

                config.UserId = currentUser.Id;
                config.AccessToken = token;
                config.UserName = currentUser.FirstName + " " + currentUser.LastName;

                await configService.SetConfig(config);

                var rootWindow = ActivatorUtilities.CreateInstance<RootWindow>(StaticService.Container);

                rootWindow.Show();
                this.Close();


            }
            catch (VkAuthException ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("ERROR IN LOGIN VIEW");
                logger.Error(ex, ex.Message);

                loading.Visibility = Visibility.Collapsed;
                content.Visibility = Visibility.Visible;

                _snackbarService.Show("Неверные данные", "Вы ввели неверно логин или пароль");
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("FATAL ERROR IN LOGIN VIEW");
                logger.Error(ex, ex.Message);

                loading.Visibility = Visibility.Collapsed;
                content.Visibility = Visibility.Visible;

                _snackbarService.Show("Ошибка", $"Произошла неизвестная ошибка при входе: {ex.Message}");
            }
        }

        public string TwoAuthDelegate()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                loading.Visibility = Visibility.Collapsed;
                content.Visibility = Visibility.Collapsed;
                TwoFactorAuth.Visibility = Visibility.Visible;
                CodeAccepted = false;
                Code.Text = string.Empty;
            });
           
            while (!CodeAccepted)
            {
                Thread.Sleep(1000);
            }

            var text = string.Empty;

            Application.Current.Dispatcher.Invoke(() =>
            {
                text = Code.Text;
            });

            return text;
        }

        private void TwoFactorAccept_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                loading.Visibility = Visibility.Visible;
                TwoFactorAuth.Visibility = Visibility.Collapsed;
                CodeAccepted = true;
            });

            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://t.me/MusicXPlayer",
                UseShellExecute = true
            });
        }
    }
}
