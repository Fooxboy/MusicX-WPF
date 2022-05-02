using DryIoc;
using MusicX.Controls.Blocks;
using MusicX.Core.Models;
using MusicX.Services;
using MusicX.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
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

        private Playlist playlist;
        private readonly long playlistId;
        private readonly long ownerId;
        private readonly string accessKey;
        private readonly Logger logger;

        private long currentUserId;

        public PlaylistView(Playlist playlist)
        {
            InitializeComponent();
            ViewModel = StaticService.Container.Resolve<PlaylistViewModel>();
            logger = StaticService.Container.Resolve<Logger>();

            ViewModel.PlaylistLoaded += ViewModel_PlaylistLoaded;


            DataContext = ViewModel;
            this.playlist = playlist;
        }

        public PlaylistView(long playlistId, long ownerId, string accessKey)
        {
            InitializeComponent();
            ViewModel = StaticService.Container.Resolve<PlaylistViewModel>();

            ViewModel.PlaylistLoaded += ViewModel_PlaylistLoaded;
            DataContext = ViewModel;
            this.playlistId = playlistId;
            this.ownerId = ownerId;
            this.accessKey = accessKey;
        }

        private void ViewModel_PlaylistLoaded(object? sender, Playlist e)
        {
            BitmapImage image = null;

            if (playlist?.Type == 1 && playlist?.AlbumType != "playlist")
            {
                image = (BitmapImage)CoverPlaylist.ImageSource;
            }



            PlaylistStackPanel.Children.Add(new AudiosListControl { BitImage = image, Audios = ViewModel.Tracks, Margin = new Thickness(0, 0, 0, 45) });

            this.playlist = e;
            if (e.OwnerId == currentUserId)
            {
                this.AddPlaylist.Content = "Удалить плейлист";
                this.AddPlaylist.Icon = WPFUI.Common.SymbolRegular.Delete24;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = await ViewModel.ConfigService.GetConfig();
                currentUserId = config.UserId;

                if(playlist != null)
                {
                    if (playlist.OwnerId == currentUserId)
                    {
                        this.AddPlaylist.Content = "Удалить плейлист";
                        this.AddPlaylist.Icon = WPFUI.Common.SymbolRegular.Delete24;
                    }
                }
               
                if (playlist != null)
                {
                    await ViewModel.LoadPlaylist(playlist);
                }
                else
                {
                    await ViewModel.LoadPlaylistFromData(playlistId, ownerId, accessKey);
                }

               

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

        private async void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if(playlist.OwnerId != currentUserId)
            {
                this.AddPlaylist.Content = "Добавление...";
                this.AddPlaylist.Icon = WPFUI.Common.SymbolRegular.Timer12;

                var res = await ViewModel.AddPlaylist();

                if (res)
                {
                    this.AddPlaylist.Content = "Удалить плейлист";
                    this.AddPlaylist.Icon = WPFUI.Common.SymbolRegular.Delete24;
                }
                else
                {
                    this.AddPlaylist.Content = "Ошибка";
                    this.AddPlaylist.Icon = WPFUI.Common.SymbolRegular.ErrorCircle20;
                }
            }else
            {
                this.AddPlaylist.Content = "Удаление...";
                this.AddPlaylist.Icon = WPFUI.Common.SymbolRegular.Timer12;

                var res = await ViewModel.RemovePlaylist();

                if (res)
                {
                    this.AddPlaylist.Content = "Добавить к себе";
                    this.AddPlaylist.Icon = WPFUI.Common.SymbolRegular.Add28;
                }
                else
                {
                    this.AddPlaylist.Content = "Ошибка";
                    this.AddPlaylist.Icon = WPFUI.Common.SymbolRegular.ErrorCircle20;
                }
            }
        }

        bool _nowPlay = false;
        private async void PlayPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var player = StaticService.Container.Resolve<PlayerService>();
                if (_nowPlay)
                {
                    _nowPlay = false;

                    PlayPlaylist.Content = "Возпроизвести";
                    PlayPlaylist.Icon = WPFUI.Common.SymbolRegular.Play20;


                    player.Pause();
                }
                else
                {
                    _nowPlay = true;

                    PlayPlaylist.Content = "Остановить воспроизведение";
                    PlayPlaylist.Icon = WPFUI.Common.SymbolRegular.Pause20;

                    await player.Play(0, ViewModel.Tracks);
                }
            }catch (Exception ex)
            {
                var logger = StaticService.Container.Resolve<Logger>();
                logger.Error(ex, ex.Message);
            }
        }

        private async void DownloadPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DownloadPlaylist.IsEnabled = false;
                var downloader = StaticService.Container.Resolve<DownloaderService>();

                await downloader.AddToQueueAsync(ViewModel.Tracks, ViewModel.Title);
            }
            catch (FileNotFoundException ex)
            {

                //go to download page

                var navigation = StaticService.Container.Resolve<Services.NavigationService>();

                navigation.NavigateToPage(new DownloadsView());
            }
        }
    }
}
