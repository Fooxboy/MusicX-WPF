using MusicX.Core.Models;

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
