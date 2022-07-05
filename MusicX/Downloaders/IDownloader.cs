using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Downloaders
{
    public interface IDownloader
    {
        public Task DownloadAsync();

        public event DownloadComplete DownloadedEvent;

        public event DownloadProgressChanged DownloadProgressChangedEvent;
    }
}
