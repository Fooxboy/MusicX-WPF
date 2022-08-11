using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
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
                    Changed("CreateIsEnable");

                }

                selectedPlaylist = value;

            }
        }

        public ICommand SelectCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        public bool IsLoading { get; set; }

        public bool CreateIsEnable { get; set; }

        private readonly NavigationService navigationService;

        private readonly VkService vkService;

        private readonly ConfigService configService;

        public PlaylistSelectorModalViewModel(NavigationService navigationService, VkService vkService, ConfigService configService)
        {
            this.navigationService = navigationService;
            this.vkService = vkService;
            this.configService = configService;

            CloseCommand = new RelayCommand(Close);
            SelectCommand = new RelayCommand(Select);
        }

        public void Select()
        {
            if(SelectedPlaylist != null)
            {
                PlaylistSelected?.Invoke(SelectedPlaylist);
            }

            this.navigationService.CloseModal();
            this.Playlists.Clear();

        }

        public void Close()
        {
            this.Playlists.Clear();
            this.navigationService.CloseModal();
        }

        public async Task LoadPlaylistsAsync()
        {
            try
            {
                IsLoading = true;
                Changed(nameof(IsLoading));

                var config = await this.configService.GetConfig();

                var playlists = await vkService.GetPlaylistsAsync(config.UserId);

                var userPlaylists = playlists.Where(p => p.Permissions.Edit);

                foreach (var plist in userPlaylists)
                {
                    Playlists.Add(plist);
                }

                Changed(nameof(Playlists));

                IsLoading = false;
                Changed(nameof(IsLoading));

            }catch(Exception ex)
            {
                
            }
           
        }
    }
}
