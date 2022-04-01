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
    /// Логика взаимодействия для RecommendedPlaylistsBlockControl.xaml
    /// </summary>
    public partial class RecommendedPlaylistsBlockControl : UserControl
    {

        public List<RecommendedPlaylist> Playlists { get; set; }
        public RecommendedPlaylistsBlockControl()
        {
            InitializeComponent();

            this.Loaded += RecommendedPlaylistsBlockControl_Loaded;
        }

        private void RecommendedPlaylistsBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(var plist in Playlists)
            {
                ListRecommendsPlist.Children.Add(new RecommendedPlaylistControl() { Playlist = plist, Width=300, Margin = new Thickness(0, 0, 10, 0) });

            }
        }
    }
}
