using Microsoft.AppCenter.Crashes;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;
using Wpf.Ui.Common;

namespace MusicX.ViewModels.Modals
{
    public class PlaylistSelectorModalViewModel : BaseViewModel
    {
        public delegate void PlaylistSelectedDelegate(Playlist playlist);

        public event PlaylistSelectedDelegate PlaylistSelected;

        public ObservableCollection<Playlist> Playlists { get; set; } = new ObservableCollection<Playlist>();

        private Playlist selectedPlaylist;
        public Playlist SelectedPlaylist
        {
            get => selectedPlaylist;
            set
            {
                if(value != null)
                {
                    CreateIsEnable = true;
                }

                selectedPlaylist = value;

            }
        }

        public ICommand SelectCommand { get; set; }

        public bool IsLoading { get; set; }

        public bool CreateIsEnable { get; set; }

        private readonly NavigationService navigationService;

        private readonly VkService vkService;

        private readonly ConfigService configService;
        private readonly Logger _logger;

        public PlaylistSelectorModalViewModel(NavigationService navigationService, VkService vkService, ConfigService configService, Logger logger)
        {
            this.navigationService = navigationService;
            this.vkService = vkService;
            this.configService = configService;
            _logger = logger;

            SelectCommand = new RelayCommand(Select);
        }

        public void Select()
        {
            if(SelectedPlaylist != null)
            {
                PlaylistSelected?.Invoke(SelectedPlaylist);
            }

            navigationService.CloseModal();
            this.Playlists.Clear();

        }

        public async Task LoadPlaylistsAsync()
        {
            try
            {
                IsLoading = true;

                var config = await this.configService.GetConfig();

                var playlists = await vkService.GetPlaylistsAsync(config.UserId);

                var userPlaylists = playlists.Where(p => p.Permissions.Edit);

                foreach (var plist in userPlaylists)
                {
                    Playlists.Add(plist);
                }

                IsLoading = false;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to load editable playlists");
            }
           
        }
    }
}
