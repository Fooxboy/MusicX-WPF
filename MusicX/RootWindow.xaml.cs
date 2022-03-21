using DryIoc;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views;
using NLog;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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

        private bool PlayerShowed = false;

        public RootWindow(NavigationService navigationService, VkService vkService, Logger logger)
        {
            InitializeComponent();     
            this.navigationService = navigationService;
            this.vkService = vkService;
            this.logger = logger;
            var playerSerivce = StaticService.Container.Resolve<PlayerService>();

            playerSerivce.TrackChangedEvent += PlayerSerivce_TrackChangedEvent;
            
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
            WPFUI.Appearance.Theme.Set(WPFUI.Appearance.ThemeType.Dark);

            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            WPFUI.Appearance.Background.Remove(windowHandle);
            WPFUI.Appearance.Background.RemoveDarkMode(windowHandle);
            //this.Background = Brushes.Transparent;

            WPFUI.Appearance.Background.Apply(windowHandle, WPFUI.Appearance.BackgroundType.Acrylic);

            logger.Info($"OS Version: {os.VersionString}");
            logger.Info($"OS Build: {os.Version.Build}");
            if(os.Version.Build >= 22000)
            {
                
                // WPFUI.Appearance.Background.Remove(windowHandle);

                logger.Info($"OS Build >= 22000, Enabled Mica");
            }

            navigationService.CurrentFrame = RootFrame;
            navigationService.SectionView = new SectionView();

            var catalogs = await vkService.GetAudioCatalogAsync();



            var icons = new List<WPFUI.Common.Icon>() 
            {
                 WPFUI.Common.Icon.MusicNote120,
                 WPFUI.Common.Icon.Headphones20,
                 WPFUI.Common.Icon.MusicNote2Play20,
                 WPFUI.Common.Icon.FoodPizza20,
                 WPFUI.Common.Icon.Play12,
                 WPFUI.Common.Icon.Star16,

            };


            var rand = new Random();

            foreach(var section in catalogs.Catalog.Sections)
            {
                var sectionPage = navigationService.SectionView;
                var number = rand.Next(0, icons.Count - 1);
                var icon = icons[number];

                icons.RemoveAt(number);

                if (section.Title.ToLower() == "моя музыка") section.Title = "Музыка";
                var navigationItem = new NavigationItem() { Tag = section.Id, Icon= icon, Content = section.Title,  Type = typeof(SectionView), Instance = sectionPage };
                navigationBar.Items.Add(navigationItem);
            }

            var item = new NavigationItem() { Tag = "test", Icon = WPFUI.Common.Icon.AppFolder24, Content = "TEST", Type = typeof(TestPage), Instance = new TestPage() };
            navigationBar.Items.Add(item);

            navigationBar.Navigated += NavigationBar_Navigated1;

            navigationBar.Navigate(catalogs.Catalog.Sections[0].Id);
        }

        private async void NavigationBar_Navigated1(WPFUI.Controls.Interfaces.INavigation navigation, WPFUI.Controls.Interfaces.INavigationItem current)
        {
            if (current.Tag == "test") return;
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
    }
}
