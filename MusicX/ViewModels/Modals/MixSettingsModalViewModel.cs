using MusicX.Core.Models.Mix;
using MusicX.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MusicX.ViewModels.Modals
{
    public class MixSettingsModalViewModel : BaseViewModel
    {

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public bool IsLoading { get; set; }


        public ObservableCollection<MixCategory> Categories { get; set; }


        private readonly VkService _vkService;

        public MixSettingsModalViewModel(VkService vkService)
        {
            _vkService = vkService;
        }


        public async Task LoadSettings(string mixId)
        {
            IsLoading = true;
            OnPropertyChanged(nameof(IsLoading));

            var settings = await _vkService.GetStreamMixSettings(mixId);

            IsLoading = false;
            OnPropertyChanged(nameof(IsLoading));


            this.Title = settings.Settings.Title;
            OnPropertyChanged(nameof(Title));

            this.Subtitle = settings.Settings.Subtitle;
            OnPropertyChanged(nameof(Subtitle));

            this.Categories = new ObservableCollection<MixCategory>(settings.Settings.Categories);
            OnPropertyChanged(nameof(Subtitle));

        }
    }
}
