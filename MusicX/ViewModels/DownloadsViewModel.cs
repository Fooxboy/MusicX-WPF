using MusicX.Commands.Downloads;
using MusicX.Core.Models;
using MusicX.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MusicX.ViewModels
{
    public class DownloadsViewModel : BaseViewModel
    {
        private readonly NotificationsService notificationsService;
        private readonly DownloaderService downloaderService;

        public bool DownloadFfmeg { get; set; }
        public ICommand DownloadAllTracks { get; set; }
        public ICommand DownloadAllPlaylists { get; set; }
        public ICommand OpenMusicDirectory { get; set; }

        public Audio NowDownloadTrack { get; set; }

        public ObservableCollection<Audio> QueueTracks { get; set; } = new ObservableCollection<Audio>();

        public DownloadsViewModel(NotificationsService notificationsService, DownloaderService downloaderService)
        {
            this.notificationsService = notificationsService;
            this.downloaderService = downloaderService;

            DownloadAllTracks = new DownloadAllTracksCommand(notificationsService, downloaderService);
            DownloadAllPlaylists = new DownloadAllPlaylistsCommand(notificationsService, downloaderService);
            OpenMusicDirectory = new OpenMusicDirectoryCommand(downloaderService);
        }

    }
}
