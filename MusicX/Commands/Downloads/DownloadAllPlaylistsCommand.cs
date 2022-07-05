using MusicX.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicX.Commands.Downloads
{
    public class DownloadAllPlaylistsCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly NotificationsService notificationsService;
        private readonly DownloaderService downloaderService;
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public DownloadAllPlaylistsCommand(NotificationsService notificationsService, DownloaderService downloaderService)
        {
            this.notificationsService = notificationsService;
            this.downloaderService = downloaderService;
        }

        public async void Execute(object? parameter)
        {
            notificationsService.Show("Добавлено", "Мы скоро начнем загрузку плейлистов");

            await downloaderService.DownloadAllPlaylistsAsync();
        }
    }
}
