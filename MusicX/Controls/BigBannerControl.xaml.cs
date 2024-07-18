using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AppCenter.Crashes;

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
            var bannerService = StaticService.Container.GetRequiredService<BannerService>();

            bannerService.ShowBannerEvent -= BannerService_ShowBannerEvent;

        }

        private void BigBannerControl_Loaded(object sender, RoutedEventArgs e)
        {

            var bannerService = StaticService.Container.GetRequiredService<BannerService>();

            CurrentBanner = Banners[0];

            bannerService.ShowBannerEvent += BannerService_ShowBannerEvent;

            this.Title.Text = Banners[0].Title;
            this.SubTitle.Text = Banners[0].Text;

            this.Description.Text = Banners[0].SubText;
            this.ImageCover.ImageSource = new BitmapImage(new Uri(Banners[0].Images.Last().Url)) { DecodePixelHeight = 600, DecodePixelWidth = 1000, CacheOption = BitmapCacheOption.None };

            Cards.Children.Clear();
            for (var index = 0; index < Banners.Count; index++)
            {
                Cards.Children.Add(new MiniCardBigBannerControl { Banner = Banners[index], IsSelected = index == 0 });
            }


            if (Banners.Count > 1)
                AutoNext();
        }

        CatalogBanner CurrentBanner;

        private void BannerService_ShowBannerEvent(CatalogBanner banner)
        {
            this.CurrentBanner = banner;
            var amim = (Storyboard)(this.Resources["OpenAnimation"]);

            amim.Completed += Amim_Completed;

            amim.Begin();
        }

        public List<CatalogBanner> Banners => ((Block)DataContext).Banners;

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((CurrentBanner.ClickAction?.Action ?? CurrentBanner.Buttons?.FirstOrDefault()?.Action) is not { } action)
                    return;
                
                var url = new Uri(action.Url);

                if (url.Segments.LastOrDefault() is not { } lastSegment || !lastSegment.Contains('_'))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = action.Url
                    });
                    return;
                }
                
                var data = lastSegment.Split("_");

                var ownerId = long.Parse(data[0]);
                var playlistId = long.Parse(data[1]);
                var accessKey = data.Length == 2 ? string.Empty : data[2];

                var notificationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

                notificationService.OpenExternalPage(new PlaylistView(playlistId, ownerId, accessKey));
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

                var logger = StaticService.Container.GetRequiredService<Logger>();

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

        bool runAutoNext;

        private async void AutoNext()
        {
            if (runAutoNext) return;
            
            await Task.Delay(6000);
            runAutoNext = true; 
            
            do
            {
                try
                {
                    var bannerService = StaticService.Container.GetRequiredService<BannerService>();

                    var currentIndex = Banners.IndexOf(CurrentBanner);

                    if (currentIndex + 1 > Banners.Count - 1)
                    {
                        currentIndex = -1;
                    }

                    bannerService.OpenBanner(Banners[currentIndex + 1]);
                }
                catch
                {
                    runAutoNext = false;
                }

                await Task.Delay(5000);
            } while (runAutoNext);

        }
    }
}
