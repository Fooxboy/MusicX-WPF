using DryIoc;
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
using WPFUI.Controls;

namespace MusicX
{
    /// <summary>
    /// Логика взаимодействия для RootWindow.xaml
    /// </summary>
    public partial class RootWindow : Window
    {
        private readonly NavigationService navigationService;
        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly ConfigService configService;

        private bool PlayerShowed = false;

        public RootWindow(NavigationService navigationService, VkService vkService, Logger logger, ConfigService configService)
        {
            InitializeComponent();     
            this.navigationService = navigationService;
            this.vkService = vkService;
            this.logger = logger;
            this.configService = configService;
            var playerSerivce = StaticService.Container.Resolve<PlayerService>();

            playerSerivce.TrackChangedEvent += PlayerSerivce_TrackChangedEvent;

            navigationService.ClosedModalWindow += NavigationService_ClosedModalWindow;
            navigationService.OpenedModalWindow += NavigationService_OpenedModalWindow;
            
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
            var os = Environment.OSVersion;

            IntPtr windowHandle = new WindowInteropHelper(this).Handle;

            WPFUI.Appearance.Background.Remove(windowHandle);

            var appTheme = WPFUI.Appearance.Theme.GetAppTheme();
            var systemTheme = WPFUI.Appearance.Theme.GetSystemTheme();
            WPFUI.Appearance.Theme.Set(
            WPFUI.Appearance.ThemeType.Dark,     // Theme type
            WPFUI.Appearance.BackgroundType.Mica, // Background type
            true                                  // Whether to change accents automatically
            );

            if (WPFUI.Appearance.Theme.IsAppMatchesSystem())
            {
                this.Background = Brushes.Transparent;
                WPFUI.Appearance.Background.Apply(windowHandle, WPFUI.Appearance.BackgroundType.Mica);

            }

            var res = WPFUI.Appearance.Theme.IsAppMatchesSystem();


            logger.Info($"OS Version: {os.VersionString}");
            logger.Info($"OS Build: {os.Version.Build}");


            if (os.Version.Build < 22000)
            {
                this.Background = (Brush)new BrushConverter().ConvertFrom("#FF202020");
            }

            navigationService.CurrentFrame = RootFrame;
            navigationService.SectionView = new SectionView();
            navigationService.SetRootWindow(this);

            var catalogs = await vkService.GetAudioCatalogAsync();
            var podcast = await vkService.GetPodcastsAsync();

            catalogs.Catalog.Sections.Add(podcast.Catalog.Sections[0]);

            var icons = new List<WPFUI.Common.Icon>() 
            {
                 WPFUI.Common.Icon.MusicNote120,
                 WPFUI.Common.Icon.Headphones20,
                 WPFUI.Common.Icon.MusicNote2Play20,
                 WPFUI.Common.Icon.FoodPizza20,
                 WPFUI.Common.Icon.Play12,
                 WPFUI.Common.Icon.Star16,
                 WPFUI.Common.Icon.PlayCircle48,

            };


            var rand = new Random();

            foreach (var section in catalogs.Catalog.Sections)
            {
                var sectionPage = navigationService.SectionView;
                var number = rand.Next(0, icons.Count);
                var icon = icons[number];

                icons.RemoveAt(number);

                if (section.Title.ToLower() == "моя музыка") section.Title = "Музыка";
                var navigationItem = new NavigationItem() { Tag = section.Id, Icon= icon, Content = section.Title,  Type = typeof(SectionView), Instance = sectionPage };
                navigationBar.Items.Add(navigationItem);
            }

            var item = new NavigationItem() { Tag = "test", Icon = WPFUI.Common.Icon.AppFolder24, Content = "TEST", Type = typeof(TestPage), Instance = new TestPage() };

            var item2 = new NavigationItem() { Tag = "settings", Icon = WPFUI.Common.Icon.Settings24, Content = "Настройки", Type = typeof(SettingsView), Instance = new SettingsView(configService) };

            navigationBar.Items.Add(item);
            navigationBar.Items.Add(item2);

            navigationBar.Navigated += NavigationBar_Navigated1;

            navigationBar.Navigate(catalogs.Catalog.Sections[0].Id);

            var thread = new Thread(CheckUpdatesInStart);
            thread.Start();
        }

        private async void NavigationBar_Navigated1(WPFUI.Controls.Interfaces.INavigation navigation, WPFUI.Controls.Interfaces.INavigationItem current)
        {
            if (current.Tag == "test" || current.Tag == "settings") return;
            await navigationService.SectionView.LoadSection((string)current.Tag);
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
            await navigationService.OpenSearchSection(null);
        }

        private async void CheckUpdatesInStart()
        {

            await Task.Delay(2000);
            var github = StaticService.Container.Resolve<GithubService>();

            var release = await github.GetLastRelease();


            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (release.TagName != StaticService.Version) navigationService.OpenModal(new AvalibleNewUpdateModal(navigationService, release), 350, 450);

            }));

        }
    }
}
