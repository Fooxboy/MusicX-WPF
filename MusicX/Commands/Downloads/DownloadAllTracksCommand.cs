using MusicX.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicX.Commands.Downloads
{
    public class DownloadAllTracksCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly NotificationsService notificationsService;
        private readonly DownloaderService downloaderService;

        public DownloadAllTracksCommand(NotificationsService notificationsService, DownloaderService downloaderService)
        {
            this.notificationsService = notificationsService;
            this.downloaderService = downloaderService;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter)
        {

            notificationsService.Show("Добавлено", "Мы скоро наченем загрузку всех треков");
            await downloaderService.DownloadAllTracks();

        }
    }
}
