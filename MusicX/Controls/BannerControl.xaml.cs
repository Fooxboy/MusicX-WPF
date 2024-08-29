using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для BannerControl.xaml
    /// </summary>
    public partial class BannerControl : UserControl
    {
        public BannerControl()
        {
            InitializeComponent();
            this.Unloaded += BannerControl_Unloaded;
        }

        private void BannerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.BannerCover.ImageSource = null;
            this.BannerCover = null;

            this.BannerTitle = null;
            this.BannerText = null;
        }

        public static readonly DependencyProperty BannerProperty =
          DependencyProperty.Register("Banner", typeof(CatalogBanner), typeof(BannerControl), new PropertyMetadata(new CatalogBanner()));

        public CatalogBanner Banner
        {
            get { return (CatalogBanner)GetValue(BannerProperty); }
            set
            {
                SetValue(BannerProperty, value);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                BannerTitle.Text = Banner.Title;
                BannerText.Text = Banner.Text;
                BannerCover.ImageSource = new BitmapImage(new Uri(Banner.Images.Last().Url)) { DecodePixelHeight = 200, DecodePixelWidth = 500, CacheOption = BitmapCacheOption.None };
            }catch (Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, ex.Message);
            }
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = new Uri(Banner.ClickAction.Action.Url);

                var data = url.Segments.LastOrDefault().Split("_");

                var ownerId = long.Parse(data[0]);
                var playlistId = long.Parse(data[1]);
                var accessKey = data[2];

                var notificationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

                notificationService.OpenExternalPage(new PlaylistView(playlistId, ownerId, accessKey));
            }
            catch (Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, "Failed to open playlist in banner control");
            }
            
        }
    }
}
