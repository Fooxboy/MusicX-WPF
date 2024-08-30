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
    /// Логика взаимодействия для TagControl.xaml
    /// </summary>
    public partial class TagControl : UserControl
    {
        public static readonly DependencyProperty TagBoomProperty = DependencyProperty.Register(
        "TagBoom", typeof(Tag), typeof(TagControl), new PropertyMetadata(new Tag() { Cover = new Avatar()}));

        public Tag TagBoom
        {
            get => (Tag)GetValue(TagBoomProperty);
            set => SetValue(TagBoomProperty, value);
        }
        public TagControl()
        {
            InitializeComponent();

            this.Loaded += TagControl_Loaded;
        }

        private void TagControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.CoverTag.ImageSource = new BitmapImage(new Uri(TagBoom.Cover.Url));
                this.Name.Text = TagBoom.Name;

                var descr = string.Empty;

                foreach (var artist in TagBoom.RelevantArtistsNames) descr += artist + ", ";

                Description.Text = descr;

                CoverBackground.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(TagBoom.Cover.AccentColor);

            }
            catch(Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();
                
                logger.Error(ex, "Failed to load tag control {Name}", TagBoom.Name);
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
