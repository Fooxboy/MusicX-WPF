using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.ViewModels;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using NLog;
using Wpf.Ui.Controls;
using NavigationService = System.Windows.Navigation.NavigationService;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для PlaylistView.xaml
    /// </summary>
    public partial class PlaylistView : Page, IProvideCustomContentState
    {
        public PlaylistViewModel ViewModel { get; set; }

        private Playlist playlist;
        private readonly long playlistId;
        private readonly long ownerId;
        private readonly string accessKey;
        private readonly Logger logger;

        private long currentUserId;
        private bool loading;

        public PlaylistView(PlaylistViewModel? viewModel = null)
        {
            ViewModel = viewModel ?? StaticService.Container.GetRequiredService<PlaylistViewModel>();;
            InitializeComponent();
            ViewModel.PlaylistLoaded += ViewModel_PlaylistLoaded;
            ViewModel.PlaylistNotLoaded += ViewModel_PlaylistNotLoaded;
            logger = StaticService.Container.GetRequiredService<Logger>();
            DataContext = ViewModel;
        }

        private void ViewModel_PlaylistNotLoaded(object? sender, Playlist e)
        {
            FuckYouVK.Visibility = Visibility.Visible;
            LoadingContentGrid.Visibility = Visibility.Collapsed;
        }

        public PlaylistView(Playlist playlist) : this()
        {
            this.playlist = playlist;
            ViewModel.PlaylistData = new(playlist.Id, playlist.OwnerId, playlist.AccessKey);
        }

        public PlaylistView(long playlistId, long ownerId, string accessKey) : this()
        {
            this.playlistId = playlistId;
            this.ownerId = ownerId;
            this.accessKey = accessKey;
            ViewModel.PlaylistData = new(playlistId, ownerId, accessKey);
        }

        private void ViewModel_PlaylistLoaded(object? sender, Playlist e)
        {
            this.playlist = e;
            if (this.playlist.Permissions.Delete)
            {
                this.AddPlaylist.Content = "Удалить плейлист";
                AddPlaylist.Icon = new SymbolIcon(SymbolRegular.Delete24);
            }

            if(!this.playlist.Permissions.Edit)
            {
                this.EditPlaylist.Visibility = Visibility.Collapsed;
            }
        }
        private async void PlaylistScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (loading || Math.Abs(e.VerticalOffset - PlaylistScrollViewer.ScrollableHeight) is > 200 or < 1)
                return;

            loading = true;
            await ViewModel.LoadMore();
            loading = false;
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
                        AddPlaylist.Icon = new SymbolIcon(SymbolRegular.Delete24);
                    }
                }

                if (ViewModel.Playlist is null)
                {
                    if (playlist != null)
                    {
                        await ViewModel.LoadPlaylist(playlist);
                    }
                    else if (accessKey != null)
                    {
                        await ViewModel.LoadPlaylistFromData(playlistId, ownerId, accessKey);
                    }
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
                AddPlaylist.Icon = new SymbolIcon(SymbolRegular.Timer12);

                var res = await ViewModel.AddPlaylist();

                if (res)
                {
                    this.AddPlaylist.Content = "Удалить плейлист";
                    AddPlaylist.Icon = new SymbolIcon(SymbolRegular.Delete24);
                }
                else
                {
                    this.AddPlaylist.Content = "Ошибка";
                    AddPlaylist.Icon = new SymbolIcon(SymbolRegular.ErrorCircle20);
                }
            }else
            {
                this.AddPlaylist.Content = "Удаление...";
                AddPlaylist.Icon = new SymbolIcon(SymbolRegular.Timer12);

                var res = await ViewModel.RemovePlaylist();

                if (res)
                {
                    this.AddPlaylist.Content = "Добавить к себе";
                    AddPlaylist.Icon = new SymbolIcon(SymbolRegular.Add28);
                }
                else
                {
                    this.AddPlaylist.Content = "Ошибка";
                    AddPlaylist.Icon = new SymbolIcon(SymbolRegular.ErrorCircle20);
                }
            }
        }

        bool _nowPlay = false;
        private async void PlayPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var player = StaticService.Container.GetRequiredService<PlayerService>();
                if (_nowPlay)
                {
                    _nowPlay = false;

                    PlayPlaylist.Content = "Возпроизвести";
                    PlayPlaylist.Icon = new SymbolIcon(SymbolRegular.Play20);


                    player.Pause();
                }
                else
                {
                    _nowPlay = true;

                    PlayPlaylist.Content = "Остановить воспроизведение";
                    PlayPlaylist.Icon = new SymbolIcon(SymbolRegular.Pause20);

                    await player.PlayAsync(
                        new VkPlaylistPlaylist(StaticService.Container.GetRequiredService<VkService>(),
                                               ViewModel.PlaylistData), ViewModel.Playlist.Audios[0].ToTrack());
                }
            }catch (Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();
                logger.Error(ex, ex.Message);
            }
        }

        private void DownloadPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DownloadPlaylist.IsEnabled = false;
                var downloader = StaticService.Container.GetRequiredService<DownloaderViewModel>();

                downloader.AddPlaylistToQueueAsync(ViewModel.Playlist.Id, ViewModel.Playlist.OwnerId, ViewModel.Playlist.AccessKey)
                    .ContinueWith(_ => downloader.StartDownloadingCommand.Execute(null))
                    .SafeFireAndForget();
            }
            catch (FileNotFoundException ex)
            {

                //go to download page

                var navigation = StaticService.Container.GetRequiredService<Services.NavigationService>();

                navigation.OpenMenuSection("downloads");
            }
        }
        public CustomContentState GetContentState()
        {
            return new PlaylistState(ViewModel);
        }

        public override string ToString()
        {
            return $"{ViewModel.PlaylistData.PlaylistId}_{ViewModel.PlaylistData.OwnerId}_{ViewModel.PlaylistData.AccessKey}";
        }

        [Serializable]
        private class PlaylistState : CustomContentState, ISerializable
        {
            private const string IdKey = "PlaylistId";
            private const string OwnerIdKey = "OwnerId";
            private const string AccessKey = "AccessKey";
            
            private readonly PlaylistViewModel _viewModel;

            public override string JournalEntryName => 
                $"{_viewModel.PlaylistData.PlaylistId}_{_viewModel.PlaylistData.OwnerId}_{_viewModel.PlaylistData.AccessKey}";
            public PlaylistState(PlaylistViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public PlaylistState(SerializationInfo info, StreamingContext context)
            {
                _viewModel = StaticService.Container.GetRequiredService<PlaylistViewModel>();

                var data = new PlaylistData(info.GetInt64(IdKey), info.GetInt64(OwnerIdKey), info.GetString(AccessKey)!);
                _viewModel.LoadPlaylistFromData(data)
                    .SafeFireAndForget();
            }
            public override void Replay(NavigationService navigationService, NavigationMode mode)
            {
                if (mode is not (NavigationMode.Forward or NavigationMode.Back))
                    navigationService.Navigate(new PlaylistView(_viewModel));
            }
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                var (id, ownerId, accessKey) = _viewModel.PlaylistData;
                
                info.AddValue(IdKey, id);
                info.AddValue(OwnerIdKey, ownerId);
                info.AddValue(AccessKey, accessKey);
            }
        }

        private void EditPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = StaticService.Container.GetRequiredService<CreatePlaylistModalViewModel>();
            viewModel.EndEvent += ViewModel_EndEvent;
            var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
            viewModel.LoadDataFromPlaylist(this.playlist);
            navigationService.OpenModal<CreatePlaylistModal>(viewModel);
        }

        private async void ViewModel_EndEvent(bool sucess)
        {
            if (!sucess) return;


            await ViewModel.LoadPlaylistFromData(this.playlist.Id, this.playlist.OwnerId, this.playlist.AccessKey);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://t.me/MusicXPlayer/132",
                UseShellExecute = true
            });
        }
    }
}
