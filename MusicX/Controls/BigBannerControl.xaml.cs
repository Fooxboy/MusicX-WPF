using DryIoc;
using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для BigBannerControl.xaml
    /// </summary>
    public partial class BigBannerControl : UserControl
    {
        public BigBannerControl()
        {
            InitializeComponent();
            this.Loaded += BigBannerControl_Loaded;
            this.Unloaded += BigBannerControl_Unloaded;
        }

        private void BigBannerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            runAutoNext = false;
        }

        private void BigBannerControl_Loaded(object sender, RoutedEventArgs e)
        {

            var bannerService = StaticService.Container.Resolve<BannerService>();

            CurrentBanner = Banners[0];

            bannerService.ShowBannerEvent += BannerService_ShowBannerEvent;

            this.Title.Text = Banners[0].Title;
            this.SubTitle.Text = Banners[0].Text;

            this.Description.Text = Banners[0].SubText;
            this.ImageCover.ImageSource = new BitmapImage(new Uri(Banners[0].Images.Last().Url)) { DecodePixelHeight = 600, DecodePixelWidth = 1000, CacheOption = BitmapCacheOption.None };

            int index = 0;
            foreach(var banner in Banners)
            {
                if(index == 0) this.Cards.Children.Add(new MiniCardBigBannerControl() { Banner = banner, IsSelected = true });
                else
                {
                    this.Cards.Children.Add(new MiniCardBigBannerControl() { Banner = banner, IsSelected = false });
                }
                index++;
            }


            new Thread(AutoNext).Start();
        }

        CatalogBanner CurrentBanner;

        private void BannerService_ShowBannerEvent(CatalogBanner banner)
        {
            this.CurrentBanner = banner;
            var amim = (Storyboard)(this.Resources["OpenAnimation"]);

            amim.Completed += Amim_Completed;

            amim.Begin();
        }

        public List<CatalogBanner> Banners { get; set; }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = new Uri(CurrentBanner.ClickAction.Action.Url);

                var data = url.Segments.LastOrDefault().Split("_");

                var ownerId = long.Parse(data[0]);
                var playlistId = long.Parse(data[1]);
                var accessKey = data[2];

                var notificationService = StaticService.Container.Resolve<Services.NavigationService>();

                notificationService.OpenExternalPage(new PlaylistView(playlistId, ownerId, accessKey));
            }
            catch (Exception ex)
            {
                var logger = StaticService.Container.Resolve<Logger>();

                logger.Error(ex, ex.Message);
            }

        }

        private void Amim_Completed(object? sender, EventArgs e)
        {
            this.Title.Text = CurrentBanner.Title;
            this.SubTitle.Text = CurrentBanner.Text;

            this.Description.Text = CurrentBanner.SubText;
            this.ImageCover.ImageSource = new BitmapImage(new Uri(CurrentBanner.Images.Last().Url)) { DecodePixelHeight = 600, DecodePixelWidth = 1000, CacheOption = BitmapCacheOption.None };

            var amim = (Storyboard)(this.Resources["CloseAnimation"]);


            amim.Begin();
        }

        bool runAutoNext = true;

        private void AutoNext()
        {

            while(runAutoNext)
            {
                Thread.Sleep(5000);
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bannerService = StaticService.Container.Resolve<BannerService>();

                        var currentIndex = Banners.IndexOf(CurrentBanner);

                        if (currentIndex + 1 > Banners.Count - 1)
                        {
                            currentIndex = -1;
                        }

                        bannerService.OpenBanner(Banners[currentIndex + 1]);

                    });
                }catch (Exception ex)
                {
                    runAutoNext = false;
                }
                
            }
           
        }
    }
}
