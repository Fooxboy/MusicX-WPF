using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : UiWindow
    {
        private readonly VkService vkService;
        private readonly ConfigService configService;
        private readonly Logger logger;
        private readonly NavigationService navigationService;
        private readonly NotificationsService notificationsService;

        private readonly bool tokenRefresh;
        public LoginWindow(VkService vkService, ConfigService configService, Logger logger, NavigationService navigationService, NotificationsService notificationsService, bool tokenRefresh = false)
        {
            InitializeComponent();
            this.vkService = vkService;
            this.configService = configService;
            this.logger = logger;
            this.navigationService = navigationService;
            this.notificationsService = notificationsService;
            this.tokenRefresh = tokenRefresh;
            this.WpfTitleBar.MaximizeClicked += WpfTitleBar_MaximizeClicked;
            Accent.Apply(Accent.GetColorizationColor(), ThemeType.Dark);
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var os = Environment.OSVersion;

            logger.Info($"OS Version: {os.VersionString}");
            logger.Info($"OS Build: {os.Version.Build}");
           

            logger.Info("Loaded Login window");

            if(tokenRefresh)
            {
                await RootSnackbar.ShowAsync("Токен устарел", "Войдите в аккаунт снова, чтобы продолжить пользоваться MusicX");
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

                var token = await vkService.AuthAsync(login, password, TwoAuthDelegate, null);
                var currentUser = await vkService.GetCurrentUserAsync();

                var config = await configService.GetConfig();

                config.UserId = currentUser.Id;
                config.AccessToken = token;
                config.UserName = currentUser.FirstName + " " + currentUser.LastName;

                await configService.SetConfig(config);

                var rootWindow = new RootWindow(navigationService, vkService, logger, configService, notificationsService);

                rootWindow.Show();
                this.Close();


            }
            catch (VkNet.AudioBypassService.Exceptions.VkAuthException ex)
            {
                logger.Error("ERROR IN LOGIN VIEW");
                logger.Error(ex, ex.Message);

                loading.Visibility = Visibility.Collapsed;
                content.Visibility = Visibility.Visible;

                await RootSnackbar.ShowAsync("Неверные данные", "Вы ввели неверно логин или пароль");
            }
            catch (Exception ex)
            {
                logger.Error("FATAL ERROR IN LOGIN VIEW");
                logger.Error(ex, ex.Message);

                loading.Visibility = Visibility.Collapsed;
                content.Visibility = Visibility.Visible;

                await RootSnackbar.ShowAsync("Ошибка", $"Произошла неизвестная ошибка при входе: {ex.Message}");
            }
        }

        public string TwoAuthDelegate()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                loading.Visibility = Visibility.Collapsed;
                content.Visibility = Visibility.Collapsed;
                TwoFactorAuth.Visibility = Visibility.Visible;
            });
           
            while (!CodeAccepted)
            {
                Task.Delay(1000);
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
    }
}
