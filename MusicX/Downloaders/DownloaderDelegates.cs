using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Downloaders
{

    public delegate void DownloadComplete(string path);

    public delegate void DownloadProgressChanged(int percent, long current, double left);
}
