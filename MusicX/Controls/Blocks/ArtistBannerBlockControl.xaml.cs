using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для ArtistBannerBlockControl.xaml
    /// </summary>
    public partial class ArtistBannerBlockControl : UserControl
    {
        public Block Block { get; set; }
        public ArtistBannerBlockControl()
        {
            InitializeComponent();
            this.Loaded += ArtistBannerBlockControl_Loaded;
        }

        private void ArtistBannerBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            ArtistBannerImage.ImageSource = new BitmapImage(new Uri(Block.Artists[0].Photo[2].Url) );
            ArtistText.Text = Block.Artists[0].Name;
        }
    }
}
