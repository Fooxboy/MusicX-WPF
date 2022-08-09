using DryIoc;
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
    /// Логика взаимодействия для ListPlaylists.xaml
    /// </summary>
    public partial class ListPlaylists : UserControl
    {

        public ListPlaylists()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PlaylistsProperty =
          DependencyProperty.Register(nameof(Playlists), typeof(IEnumerable<Playlist>), typeof(ListPlaylists), new PropertyMetadata(Array.Empty<Playlist>()));

        public IEnumerable<Playlist> Playlists
        {
            get => (List<Playlist>)GetValue(PlaylistsProperty);
            set => SetValue(PlaylistsProperty, value);
        }

        public static readonly DependencyProperty ShowFullProperty = DependencyProperty.Register(
            nameof(ShowFull), typeof(bool), typeof(ListPlaylists));

        public bool ShowFull
        {
            get => (bool)GetValue(ShowFullProperty);
            set => SetValue(ShowFullProperty, value);
        }
    }
}
