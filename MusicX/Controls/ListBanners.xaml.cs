using DryIoc;
using HandyControl.Controls;
using MusicX.Core.Models;
using MusicX.Services;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            logger = StaticService.Container.Resolve<Logger>();
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
                    StackPanelBanners.Items.Add(new BannerControl() { Banner = banner, Height = 200, Width = 500, Margin = new Thickness(0, 0, 10, 30) });
                    //StackPanelBanners.Children.Add(new BannerControl() { Banner = banner, Height = 200, Width = 500, Margin = new Thickness(0, 0, 10, 0) });
                }
            }catch (Exception ex)
            {
                logger.Error("Failed load list banners control");
                logger.Error(ex, ex.Message);
            }
            
        }
    }
}
