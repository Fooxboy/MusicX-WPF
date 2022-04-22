using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Services
{
    public class BannerService
    {
        public event ShowBanner ShowBannerEvent;

        public delegate void ShowBanner(CatalogBanner banner);



        public void OpenBanner(CatalogBanner banner)
        {
            ShowBannerEvent?.Invoke(banner);
        }
    }
}
