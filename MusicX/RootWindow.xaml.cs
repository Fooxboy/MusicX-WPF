using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using MusicX.Controls;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Shared.Player;
using MusicX.ViewModels;
using MusicX.Views;
using MusicX.Views.Modals;
using NLog;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using Wpf.Ui.Tray.Controls;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX
{
    /// <summary>
    /// Логика взаимодействия для RootWindow.xaml
    /// </summary>
    public partial class RootWindow
    {
        private readonly NavigationService navigationService;
        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly ConfigService configService;
        private readonly ISnackbarService _snackbarService;

        public static SnowEngine SnowEngine = null;

        public static bool WinterTheme = false;


        private bool PlayerShowed = false;

        public RootWindow(NavigationService navigationService, VkService vkService, Logger logger,
            ConfigService configService, ISnackbarService snackbarService,
                          ListenTogetherService togetherService, WindowThemeService themeService) : base(snackbarService, navigationService, themeService)
        {
            Application.Current.MainWindow = this;
            InitializeComponent();     
            this.navigationService = navigationService;
            this.vkService = vkService;
            this.logger = logger;
            this.configService = configService;
            _snackbarService = snackbarService;
            var playerSerivce = StaticService.Container.GetRequiredService<PlayerService>();

            playerSerivce.TrackChangedEvent += PlayerSerivce_TrackChangedEvent;

            SingleAppService.Instance.RunWitchArgs += Instance_RunWitchArgs;
            
            togetherService.ConnectedToSession += TogetherServiceOnConnectedToSession;

            Width = configService.Config.Width;
            Height = configService.Config.Height;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            
            if (WindowState == WindowState.Minimized && TrayIcon.IsRegistered && ConsiderHideToTray()) 
                Hide();
        }

        protected override SnackbarPresenter? GetSnackbarPresenter() => RootSnackbar;

        private async Task TogetherServiceOnConnectedToSession(PlaylistTrack arg)
        {
            await Dispatcher.InvokeAsync(Activate);
        }

        private async Task Instance_RunWitchArgs(string[] arg)
        {
            try
            {
                var a = arg[0].Split(':');
                if (a[0] == "musicxshare")
                {
                    await StartListenTogether(a[1]);
                }
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);

                _snackbarService.Show("Ошибка", "Мы не смогли подключится к сервису совместного прослушивания");
            }
           
        }

        public async Task StartListenTogether(string sessionId)
        {
            var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();

            await Task.Factory.StartNew(async() =>
            {
                try
                {
                    var connectionService = StaticService.Container.GetRequiredService<BackendConnectionService>();
                    connectionService.ReportMetric("ConnectToSession");

                    var config = await configService.GetConfig();
                    await listenTogetherService.ConnectToServerAsync(config.UserId);
                    await listenTogetherService.JoinToSesstionAsync(sessionId);
                }catch(Exception ex)
                {
                    logger.Error(ex, ex.Message);
                }
                
            });
          
        }

        private bool ConsiderHideToTray()
        {
            if (configService.Config.MinimizeToTray is not null) return configService.Config.MinimizeToTray.Value;

            new ToastContentBuilder()
                .AddText("Приложене было скрыто в трее, нажмите на иконку, чтобы снова открыть окно.")
                .AddText("Поведение можно изменить в настройках. Это уведомление больше не будет показано.")
                .Show();
                
            configService.Config.MinimizeToTray = true;
            configService.SetConfig(configService.Config).SafeFireAndForget();

            return true;
        }

        private void PlayerSerivce_TrackChangedEvent(object? sender, EventArgs e)
        {
            if (PlayerShowed) return;
            playerControl.Visibility = Visibility.Visible;
            var amim = (Storyboard)(this.Resources["AminationShowPlayer"]);
            amim.Begin();
            PlayerShowed = true;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var os = Environment.OSVersion;

                logger.Info($"OS Version: {os.VersionString}");
                logger.Info($"OS Build: {os.Version.Build}");
                
                navigationService.MenuSectionOpened += NavigationServiceOnMenuSectionOpened;
                navigationService.ExternalSectionOpened += NavigationServiceOnExternalSectionOpened;
                navigationService.BackRequested += NavigationServiceOnBackRequested;
                navigationService.ExternalPageOpened += NavigationServiceOnExternalPageOpened;
                navigationService.ReplaceBlocksRequested += NavigationServiceOnReplaceBlocksRequested;

                var catalogs = await vkService.GetAudioCatalogAsync();

                var icons = new List<SymbolRegular>
                {
                    SymbolRegular.MusicNote120,
                    SymbolRegular.Headphones20,
                    SymbolRegular.MusicNote2Play20,
                    SymbolRegular.FoodPizza20,
                    SymbolRegular.Play12,
                    SymbolRegular.Star16,
                    SymbolRegular.PlayCircle48,
                    SymbolRegular.HeadphonesSoundWave20,
                    SymbolRegular.Speaker228
                };

                var updatesSection = await vkService.GetAudioCatalogAsync("https://vk.com/audio?section=updates");
                
                if (updatesSection.Catalog?.Sections?.Count > 0)
                {
                    var section = updatesSection.Catalog.Sections[0];
                    section.Title = "Подписки";
                    catalogs.Catalog.Sections.Insert(catalogs.Catalog.Sections.Count - 1, section);
                }

                var sectionsService = StaticService.Container.GetRequiredService<ICustomSectionsService>();
                
                catalogs.Catalog.Sections.AddRange(await sectionsService.GetSectionsAsync().ToArrayAsync());

                var rand = new Random();

                foreach (var section in catalogs.Catalog.Sections)
                {
                    SymbolRegular icon;

                    if (section.Title.Equals("главная", StringComparison.CurrentCultureIgnoreCase))
                    {
                        icon = SymbolRegular.Home24;
                    }
                    else if (section.Title.Equals("моя музыка", StringComparison.CurrentCultureIgnoreCase))
                    {
                        icon = SymbolRegular.MusicNote120;
                    }
                    else if (section.Title.Equals("обзор", StringComparison.CurrentCultureIgnoreCase))
                    {
                        icon = SymbolRegular.CompassNorthwest28;
                    }
                    else if (section.Title.Equals("подкасты", StringComparison.CurrentCultureIgnoreCase))
                    {
                        icon = SymbolRegular.HeadphonesSoundWave20;
                    }
                    else if (section.Title.Equals("подписки", StringComparison.CurrentCultureIgnoreCase))
                    {
                        icon = SymbolRegular.Feed24;
                    }
                    else if (section.Title.Equals("каталоги", StringComparison.CurrentCultureIgnoreCase))
                    {
                        icon = SymbolRegular.People24;
                    }
                    else if (section.Title.Equals("поиск", StringComparison.CurrentCultureIgnoreCase))
                    {
                        icon = SymbolRegular.Search24;
                    }
                    else if (section.Title.StartsWith("книги", StringComparison.CurrentCultureIgnoreCase) || 
                             section.Title.StartsWith("радио", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    else
                    {
                        var number = rand.Next(0, icons.Count);
                        icon = icons[number];
                        icons.RemoveAt(number);
                    }



                    if (section.Title.ToLower() == "моя музыка") section.Title = "Музыка";

                    var viewModel = ActivatorUtilities.CreateInstance<SectionViewModel>(StaticService.Container);
                    viewModel.SectionId = section.Id;

                    var navigationItem = new NavigationBarItem() { Tag = section.Id, PageDataContext = viewModel, Icon = icon, Content = section.Title, PageType = typeof(SectionView) };
                    navigationBar.Items.Add(navigationItem);
                }

#if DEBUG
                var item = new NavigationBarItem
                    { Tag = "test", Icon = SymbolRegular.AppFolder24, Content = "TEST", PageType = typeof(TestPage) };
                navigationBar.Items.Add(item);
#endif
                navigationBar.Items.Add(new()
                {
                    Tag = "vkmix", PageDataContext = StaticService.Container.GetRequiredService<VKMixViewModel>(),
                    Icon = SymbolRegular.Stream24, Content = "Микс", PageType = typeof(VKMixView)
                });
                navigationBar.Items.Add(new()
                {
                    Tag = "downloads",
                    PageDataContext = StaticService.Container.GetRequiredService<DownloaderViewModel>(),
                    Icon = SymbolRegular.ArrowDownload48, Content = "Загрузки", PageType = typeof(DownloadsView)
                });
                var item2 = new NavigationBarItem
                {
                    Tag = "settings", Icon = SymbolRegular.Settings24, Content = "Настройки",
                    PageType = typeof(SettingsView)
                };

                navigationBar.Items.Add(item2);

                navigationBar.Items[0].RaiseEvent(new(ButtonBase.ClickEvent));

#if !DEBUG
                CheckUpdatesInStart().SafeFireAndForget();
#endif

                var config = await configService.GetConfig();

                if (config.AnimatedBackground is null) config.AnimatedBackground = false;

                if (config.AnimatedBackground == true)
                {
                    SnowEngine = new SnowEngine(canvas, "pack://application:,,,/Assets/newyear/snow1.png",
                                              "pack://application:,,,/Assets/newyear/snow2.png",
                                              "pack://application:,,,/Assets/newyear/snow3.png",
                                              "pack://application:,,,/Assets/newyear/snow4.png",
                                              "pack://application:,,,/Assets/newyear/snow5.png",
                                              "pack://application:,,,/Assets/newyear/snow6.png",
                                              "pack://application:,,,/Assets/newyear/snow7.png",
                                              "pack://application:,,,/Assets/newyear/snow8.png",
                                              "pack://application:,,,/Assets/newyear/snow9.png");
                    SnowEngine.Start();
                }

                if (config.WinterTheme is null) config.WinterTheme = false;


                WinterTheme = config.WinterTheme.Value;
                if(config.WinterTheme == true)
                {
                    WinterBackground.Visibility = Visibility.Visible;
                }

                if (config.NotifyMessages is null)
                    config.NotifyMessages = new() { ShowListenTogetherModal = true, LastShowedTelegramBlock = null };

                if (config.LastPlayerState is not null)
                {
                    await playerControl.PlayerService.RestoreFromStateAsync(config.LastPlayerState);

                    config.LastPlayerState = null;
                }

                await configService.SetConfig(config);

                if(config.NotifyMessages.ShowListenTogetherModal)
                {
                    navigationService.OpenModal<WelcomeToListenTogetherModal>();
                }

                this.WindowState = WindowState.Normal;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load root window");
                _snackbarService.Show("Ошибка запуска",
                    "Попробуйте перезапустить приложение, если ошибка повторяется, напишите об этом разработчику");
            }
            
        }
        private void NavigationServiceOnReplaceBlocksRequested(object? sender, string e)
        {
            if (RootFrame.GetDataContext() is SectionViewModel viewModel)
                viewModel.ReplaceBlocks(e).SafeFireAndForget();
        }
        private void NavigationServiceOnExternalPageOpened(object? sender, object e)
        {
            RootFrame.Navigate(e);
        }
        private void NavigationServiceOnBackRequested(object? sender, EventArgs e)
        {
            if (!RootFrame.CanGoBack)
                return;
            
            RootFrame.GoBack();
            if (RootFrame.RemoveBackEntry().CustomContentState is IDisposable disposable)
                disposable.Dispose();
        }
        private void NavigationServiceOnExternalSectionOpened(object? sender, SectionViewModel e)
        {
            RootFrame.Navigate(new SectionView
            {
                DataContext = e
            });
        }
        private void NavigationServiceOnMenuSectionOpened(object? sender, string s)
        {
            navigationBar.Items.First(b => b.Tag is string tag && tag == s)
                .RaiseEvent(new(ButtonBase.ClickEvent));
        }


        private async void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SearchBox.Text == String.Empty) return;

                var query = SearchBox.Text;

                navigationService.OpenSection(query, SectionType.Search);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            navigationService.GoBack();
        }

        private void playerControl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void playerControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        //    if (!PlayerShowed) return;
        //    var amim = (Storyboard)(this.Resources["HidePlayer"]);
        //    amim.Begin();

        //    playerControl.HorizontalAlignment = HorizontalAlignment.Left;
        }

        private void SearchBox_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.IBeam;

        }

        private void SearchBox_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;

        }

        private async void SearchBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                navigationService.OpenSection(null, SectionType.Search);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to open empty search section");

                _snackbarService.Show("Ошибка открытия поиска", "Мы не смогли открыть подсказки поиска");
            }
        }

        private async Task CheckUpdatesInStart()
        {

            try
            {
                await Task.Delay(2000);
                
                var updateService = StaticService.Container.GetRequiredService<UpdateService>();
                
                await updateService.CheckForUpdates();
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Failed to check updates on start");

                _snackbarService.Show("Ошибка проверки обновлений", "Мы не смогли проверить доступные обновления");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {

                var playerService = StaticService.Container.GetRequiredService<PlayerService>();

                if (playerService == null) return;

                if (playerService.CurrentTrack == null) return;

                if (playerService.IsPlaying) playerService.Pause();
                else playerService.Play();
            }
        }
        private void Previous_OnClick(object? sender, EventArgs e)
        {
            var playerService = StaticService.Container.GetRequiredService<PlayerService>();
            if (playerService.Tracks.Count > 0)
                playerService.PreviousTrack().SafeFireAndForget();
        }
        private void PlayPause_OnClick(object? sender, EventArgs e)
        {
            var playerService = StaticService.Container.GetRequiredService<PlayerService>();
            if (playerService.CurrentTrack is null)
                return;
            
            if (playerService.IsPlaying)
                playerService.Pause();
            else
                playerService.Play();
        }
        private void Next_OnClick(object? sender, EventArgs e)
        {
            var playerService = StaticService.Container.GetRequiredService<PlayerService>();
            if (playerService.Tracks.Count > 0)
                playerService.NextTrack().SafeFireAndForget();
        }

        /*private void MenuItem_Click(object sender, RoutedEventArgs e) // TODO tray
        {
            WpfTitleBar.MinimizeToTray = false;
            this.Show();

            this.WindowState = WindowState.Normal;

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            WpfTitleBar.MinimizeToTray = true;
            this.WindowState = WindowState.Minimized;

            this.Hide();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //toodo
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NotifyIcon_LeftClick(NotifyIcon sender, RoutedEventArgs e)
        {
            WpfTitleBar.MinimizeToTray = false;

            this.Show();

            this.WindowState = WindowState.Normal;

        }*/

        private async void RootWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            configService.Config.Width = Width;
            configService.Config.Height = Height;

            if (configService.Config.SavePlayerState is true)
                configService.Config.LastPlayerState = PlayerState.CreateOrNull(playerControl.PlayerService);

            await configService.SetConfig(configService.Config);
            
            try
            {
                var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();

                if (listenTogetherService.IsConnectedToServer && listenTogetherService.PlayerMode != PlayerMode.None)
                {
                    if (listenTogetherService.PlayerMode == PlayerMode.Owner)
                    {
                        await listenTogetherService.StopPlaySessionAsync();
                    }
                    else
                    {
                        await listenTogetherService.LeavePlaySessionAsync();
                    }
                }
            }catch(Exception ex)
            {
                //nothing
            }
        }

        private void RootFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            SearchBox.Visibility = e.Content is SectionView { DataContext: SectionViewModel { SectionId: "search" } or SectionViewModel { SectionType: SectionType.Search or SectionType.SearchResult } }
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TrayIcon_OnLeftClick(NotifyIcon sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }
    }
}
