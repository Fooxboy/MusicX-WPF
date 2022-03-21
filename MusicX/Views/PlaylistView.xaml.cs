using DryIoc;
using MusicX.Controls.Blocks;
using MusicX.Core.Models;
using MusicX.Services;
using MusicX.ViewModels;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для PlaylistView.xaml
    /// </summary>
    public partial class PlaylistView : Page
    {
        public PlaylistViewModel ViewModel { get; set; }

        private readonly Playlist playlist;
        private readonly long playlistId;
        private readonly long ownerId;
        private readonly string accessKey;
        private readonly Logger logger;

        public PlaylistView(Playlist playlist)
        {
            InitializeComponent();
            ViewModel = StaticService.Container.Resolve<PlaylistViewModel>();
            logger = StaticService.Container.Resolve<Logger>();

            DataContext = ViewModel;
            this.playlist = playlist;
        }

        public PlaylistView(long playlistId, long ownerId, string accessKey)
        {
            InitializeComponent();
            ViewModel = StaticService.Container.Resolve<PlaylistViewModel>();

            DataContext = ViewModel;
            this.playlistId = playlistId;
            this.ownerId = ownerId;
            this.accessKey = accessKey;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (playlist != null)
                {
                    await ViewModel.LoadPlaylist(playlist);
                }
                else
                {
                    await ViewModel.LoadPlaylistFromData(playlistId, ownerId, accessKey);
                }

                BitmapImage image = null;

                if(playlist?.Type == 1 && playlist?.AlbumType != "playlist")
                {
                    image = (BitmapImage)CoverPlaylist.ImageSource;
                }

               

                PlaylistStackPanel.Children.Add(new AudiosListControl { BitImage = image, Audios = ViewModel.Tracks, Margin = new Thickness(0,0,0, 90)});

                CardPlaylist.Visibility = Visibility.Visible;
                var amim = (Storyboard)(this.Resources["LoadedPlaylist"]);
                amim.Begin();
            }
            catch(Exception ex)
            {
                logger.Error("FATAL ERROR IN PLAYLIST PAGE LOADED");
                logger.Error(ex, ex.Message);
            }
           
        }
    }
}
