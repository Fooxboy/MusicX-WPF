using MusicX.Core.Models.Boom;
using MusicX.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace MusicX.Controls.Boom
{
    /// <summary>
    /// Логика взаимодействия для ArtistControl.xaml
    /// </summary>
    public partial class ArtistControl : UserControl
    {
        public static readonly DependencyProperty ArtistProperty = DependencyProperty.Register(
          "Artist", typeof(Artist), typeof(ArtistControl), new PropertyMetadata(new Artist() { Avatar = new Avatar()}));

        public Artist Artist
        {
            get => (Artist)GetValue(ArtistProperty);
            set => SetValue(ArtistProperty, value);
        }

        public ArtistControl()
        {
            InitializeComponent();

            this.Loaded += ArtistControl_Loaded;
        }

        private void ArtistControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.CoverArtist.ImageSource = new BitmapImage(new Uri(Artist.Avatar.Url));
                this.Name.Text = Artist.Name;

                var descr = string.Empty;

                if(Artist.RelevantArtistsNames != null) foreach(var artist in Artist.RelevantArtistsNames) descr += artist + ", ";

                Description.Text = descr;

                CoverBackground.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(Artist.Avatar.AccentColor);
            }
            catch(Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();
                
                logger.Error(ex, "Failed to load artist control {ArtistName}", Artist.Name);
            }
          
        }

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CardBackground.Opacity = 0.5;
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CardBackground.Opacity = 1;

        }
    }
}
