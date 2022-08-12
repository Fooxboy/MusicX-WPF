using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl
    {
        public static readonly DependencyProperty VideoProperty = DependencyProperty.Register(
            nameof(Video), typeof(Video), typeof(VideoControl));

        public Video Video
        {
            get => (Video)GetValue(VideoProperty);
            set => SetValue(VideoProperty, value);
        }
        public VideoControl()
        {
            InitializeComponent();
            this.Loaded += VideoControl_Loaded;
        }

        private void VideoControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                VideoImage.ImageSource = new BitmapImage(new Uri(Video.Image[3].Url));
                Time.Text = TimeSpan.FromSeconds(Video.Duration).ToString("m\\:ss");
                NameVideo.Text = Video.Title;
                AuthorVideo.Text = Video.MainArtists[0].Name;
            }catch (Exception ex)
            {
                NameVideo.Text = Video.Title;
                AuthorVideo.Text = "";
            }
           
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;

        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Video.Player,
                UseShellExecute = true
            });
        }
    }
}
