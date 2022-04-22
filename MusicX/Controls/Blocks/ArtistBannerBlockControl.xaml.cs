using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для ArtistBannerBlockControl.xaml
    /// </summary>
    public partial class ArtistBannerBlockControl : UserControl
    {
        public Block Block { get; set; }
        public ArtistBannerBlockControl(Block block)
        {
            this.Loaded += ArtistBannerBlockControl_Loaded;
            this.Initialized += ArtistBannerBlockControl_Initialized;
            InitializeComponent();

            ArtistBannerImage.ImageSource = new BitmapImage(new Uri(block.Artists[0].Photo[2].Url));
            ArtistText.Text = block.Artists[0].Name;

            ArtistText.Visibility = Visibility.Collapsed;
            ArtistBanner.Visibility = Visibility.Collapsed;
        }

        private void ArtistBannerBlockControl_Initialized(object? sender, EventArgs e)
        {
            

        }

        private void ArtistBannerBlockControl_Loaded(object sender, RoutedEventArgs e)
        {

            new Thread(()=>
            {
                
                Thread.Sleep(800);

                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ArtistText.Visibility = Visibility.Visible;
                    ArtistBanner.Visibility = Visibility.Visible;
                    var amim = (Storyboard)(this.Resources["OpenAnimation"]);
                    amim.Begin();
                });

               
            }).Start(); 

            

        }

        private void ActionArtistButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var amim = (Storyboard)(this.Resources["OpenAnimation"]);
            amim.Begin();
        }
    }
}
