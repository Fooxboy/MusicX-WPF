using DryIoc;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views;
using MusicX.Views.Modals;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AsyncAwaitBestPractices;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MusicX
{
    /// <summary>
    /// Логика взаимодействия для RootWindow.xaml
    /// </summary>
    public partial class RootWindow : UiWindow
    {
        private readonly NavigationService navigationService;
        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly ConfigService configService;
        private readonly NotificationsService notificationsService;

        private bool PlayerShowed = false;

        public RootWindow(NavigationService navigationService, VkService vkService, Logger logger, ConfigService configService, NotificationsService notificationsService)
        {
            //Style = "{StaticResource UiWindow}"

            InitializeComponent();     
            this.navigationService = navigationService;
            this.vkService = vkService;
            this.logger = logger;
            this.configService = configService;
            this.notificationsService = notificationsService;
            var playerSerivce = StaticService.Container.Resolve<PlayerService>();

            playerSerivce.TrackChangedEvent += PlayerSerivce_TrackChangedEvent;

            navigationService.ClosedModalWindow += NavigationService_ClosedModalWindow;
            navigationService.OpenedModalWindow += NavigationService_OpenedModalWindow;


            notificationsService.NewNotificationEvent += NotificationsService_NewNotificationEvent;

            Accent.Apply(Accent.GetColorizationColor(), ThemeType.Dark);
        }



        //private bool isFullScreen = false;
        //private void WpfTitleBar_MaximizeClicked(object sender, RoutedEventArgs e)
        //{
        //    isFullScreen = !isFullScreen;
        //    if(isFullScreen)
        //    {
        //        rootGrid.Margin = new Thickness(8,8,8,0);

        //    }else
        //    {
        //        rootGrid.Margin = new Thickness(0, 0, 0, 0);
        //    }
        //}

        private async void NotificationsService_NewNotificationEvent(string title, string message)
        {
            RootSnackbar.Title = title;
            RootSnackbar.Message = message;
            await RootSnackbar.ShowAsync();
        }

        private void NavigationService_OpenedModalWindow(object Page, int height, int width)
        {
            ModalGrid.Visibility = Visibility.Visible;
            ModalFrame.Height = height;
            ModalFrame.Width = width;
            ModalFrame.Navigate(Page);
        }

        private void NavigationService_ClosedModalWindow()
        {
            ModalGrid.Visibility = Visibility.Collapsed;
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

                navigationService.CurrentFrame = RootFrame;
                navigationService.SectionView = new SectionView();
                navigationService.SetRootWindow(this);

                var catalogs = await vkService.GetAudioCatalogAsync();

                var icons = new List<Wpf.Ui.Common.SymbolRegular>()
                {
                    Wpf.Ui.Common.SymbolRegular.MusicNote120,
                    Wpf.Ui.Common.SymbolRegular.Headphones20,
                    Wpf.Ui.Common.SymbolRegular.MusicNote2Play20,
                    Wpf.Ui.Common.SymbolRegular.FoodPizza20,
                    Wpf.Ui.Common.SymbolRegular.Play12,
                    Wpf.Ui.Common.SymbolRegular.Star16,
                    Wpf.Ui.Common.SymbolRegular.PlayCircle48,
                    Wpf.Ui.Common.SymbolRegular.HeadphonesSoundWave20,
                    Wpf.Ui.Common.SymbolRegular.Speaker228,


                };

                var updatesSection = await vkService.GetAudioCatalogAsync("https://vk.com/audio?section=updates");
                
                if (updatesSection.Catalog?.Sections?.Count > 0)
                {
                    var section = updatesSection.Catalog.Sections[0];
                    section.Title = "Подписки";
                    catalogs.Catalog.Sections.Insert(catalogs.Catalog.Sections.Count - 1, section);
                }

                var rand = new Random();

                foreach (var section in catalogs.Catalog.Sections)
                {
                    var sectionPage = navigationService.SectionView;

                    Wpf.Ui.Common.SymbolRegular icon;

                    if (section.Title.ToLower() == "главная") icon = Wpf.Ui.Common.SymbolRegular.Home24;
                    else if (section.Title.ToLower() == "моя музыка") icon = Wpf.Ui.Common.SymbolRegular.MusicNote120;
                    else if (section.Title.ToLower() == "обзор") icon = Wpf.Ui.Common.SymbolRegular.CompassNorthwest28;
                    else if (section.Title.ToLower() == "подкасты") icon = Wpf.Ui.Common.SymbolRegular.HeadphonesSoundWave20;
                    else if (section.Title.ToLower() == "подписки") icon = Wpf.Ui.Common.SymbolRegular.Feed24;
                    else
                    {
                        var number = rand.Next(0, icons.Count);
                        icon = icons[number];
                        icons.RemoveAt(number);
                    }



                    if (section.Title.ToLower() == "моя музыка") section.Title = "Музыка";


                    var navigationItem = new NavigationItem() { PageTag = section.Id, Icon = icon, Content = section.Title, PageType = typeof(SectionView) };
                    navigationBar.Items.Add(navigationItem);
                }

#if DEBUG
                var item = new NavigationItem() { PageTag = "test", Icon = Wpf.Ui.Common.SymbolRegular.AppFolder24, Content = "TEST", PageType = typeof(TestPage) };
                navigationBar.Items.Add(item);
#endif

                navigationBar.Items.Add(new NavigationItem() { PageTag = "downloads", Icon = Wpf.Ui.Common.SymbolRegular.ArrowDownload48, Content = "Загрузки", PageType = typeof(DownloadsView) });
                var item2 = new NavigationItem() { PageTag = "settings", Icon = Wpf.Ui.Common.SymbolRegular.Settings24, Content = "Настройки", PageType = typeof(SettingsView) };

                navigationBar.Items.Add(item2);

                navigationBar.Navigated += NavigationBar_Navigated;

                navigationBar.Navigate(catalogs.Catalog.Sections[0].Id);


                var thread = new Thread(CheckUpdatesInStart);
                thread.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                notificationsService.Show("Ошибка запуска", "Попробуйте перезапустить приложение, если ошибка повторяется, напишите об этом разработчику");
            }
            
        }

        private async void NavigationBar_Navigated([System.Diagnostics.CodeAnalysis.NotNull] Wpf.Ui.Controls.Interfaces.INavigation sender, Wpf.Ui.Common.RoutedNavigationEventArgs e)
        {
            var current = e.CurrentPage;

            if (current.PageTag == "test" || current.PageTag == "settings" || current.PageTag == "downloads") return;
            await navigationService.SectionView.LoadSection((string)current.PageTag);
        }

      

        private async void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SearchBox.Text == String.Empty) return;

                var query = SearchBox.Text;

                await navigationService.OpenSearchSection(query);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await navigationService.Back();
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
                await navigationService.OpenSearchSection(null);

            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);

                notificationsService.Show("Ошибка открытия поиска", "Мы не смогли открыть подсказки поиска");


            }
        }

        private async void CheckUpdatesInStart()
        {

            try
            {
                await Task.Delay(2000);
                var github = StaticService.Container.Resolve<GithubService>();

                var release = await github.GetLastRelease();


                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (release.TagName != StaticService.Version) navigationService.OpenModal(new AvalibleNewUpdateModal(navigationService, release), 350, 450);

                }));
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);

                notificationsService.Show("Ошибка проверки обновлений", "Мы не смогли проверить доступные обновления");
            }
           

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {

                var playerService = StaticService.Container.Resolve<PlayerService>();

                if (playerService == null) return;

                if (playerService.CurrentTrack == null) return;

                if (playerService.IsPlaying) playerService.Pause();
                else playerService.Play();
            }
        }
        private void Previous_OnClick(object? sender, EventArgs e)
        {
            var playerService = StaticService.Container.Resolve<PlayerService>();
            if (playerService.Tracks.Count > 0 && playerService.Tracks.IndexOf(playerService.CurrentTrack) > 0)
                playerService.PreviousTrack().SafeFireAndForget();
        }
        private void PlayPause_OnClick(object? sender, EventArgs e)
        {
            var playerService = StaticService.Container.Resolve<PlayerService>();
            if (playerService.CurrentTrack is null)
                return;
            
            if (playerService.IsPlaying)
                playerService.Pause();
            else
                playerService.Play();
        }
        private void Next_OnClick(object? sender, EventArgs e)
        {
            var playerService = StaticService.Container.Resolve<PlayerService>();
            if (playerService.Tracks.Count > 0 && playerService.Tracks.IndexOf(playerService.CurrentTrack) < playerService.Tracks.Count)
                playerService.NextTrack().SafeFireAndForget();
        }
    }
}
