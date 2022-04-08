using MusicX.Core.Models;
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

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для VideosSliderBlockControl.xaml
    /// </summary>
    public partial class VideosSliderBlockControl : UserControl
    {

        public List<Video> Videos;
        public bool ShowFull;

        public VideosSliderBlockControl()
        {
            InitializeComponent();
            this.Loaded += VideosSliderBlockControl_Loaded;
        }

        private void VideosSliderBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(ShowFull)
            {
                ListAllVideos.Orientation = Orientation.Vertical;
            }
            foreach(var video in Videos)
            {
                ListAllVideos.Children.Add(new VideoControl() { Height = 200, Width = 300, Video = video, Margin = new Thickness(0, 0, 10, 0) });
            }
        }
    }
}
