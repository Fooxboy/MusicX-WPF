using MusicX.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicX.Commands.Downloads
{
    public class OpenMusicDirectoryCommand : ICommand
    {
        private readonly DownloaderService downloaderService;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public OpenMusicDirectoryCommand(DownloaderService downloaderService)
        {
            this.downloaderService = downloaderService;
        }

        public void Execute(object? parameter)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = downloaderService.musicFolder,
                UseShellExecute = true
            });
        }
    }
}
