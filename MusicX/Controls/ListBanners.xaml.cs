using MusicX.Core.Models;
using MusicX.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AppCenter.Crashes;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для ListBanners.xaml
    /// </summary>
    public partial class ListBanners : UserControl
    {
        private readonly Logger logger;

        public ListBanners()
        {
            InitializeComponent();
            logger = StaticService.Container.GetRequiredService<Logger>();
            this.Unloaded += ListBanners_Unloaded;

        }

        private void ListBanners_Unloaded(object sender, RoutedEventArgs e)
        {
            this.StackPanelBanners.Items.Clear();
        }

        public static readonly DependencyProperty BannersProperty =
          DependencyProperty.Register("Banners", typeof(List<CatalogBanner>), typeof(ListBanners), new PropertyMetadata(new List<CatalogBanner>()));

        public List<CatalogBanner> Banners
        {
            get { return (List<CatalogBanner>)GetValue(BannersProperty); }
            set
            {
                SetValue(BannersProperty, value);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var banner in Banners)
                {
                    StackPanelBanners.Items.Add(new BannerControl() { Banner = banner, Height = 250, Width = 500, Margin = new Thickness(0, 0, 10, 30) });
                    //StackPanelBanners.Children.Add(new BannerControl() { Banner = banner, Height = 200, Width = 500, Margin = new Thickness(0, 0, 10, 0) });
                }
            }catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("Failed load list banners control");
                logger.Error(ex, ex.Message);
            }
            
        }
    }
}
