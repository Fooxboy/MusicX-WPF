using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using NLog;
using Wpf.Ui;
using Wpf.Ui.Common;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.ViewModels.Modals
{
    public class TracksSelectorModalViewModel : BaseViewModel
    {
        public IList SelectedTracks { get; set; } 

        public ObservableCollection<Audio> Tracks { get; } = new ObservableCollection<Audio>();

        public bool IsLoading { get; set; } = true;

        public ICommand ConfirmCommand { get; }
        
        public ICommand CancelCommand { get; }

        public delegate void TracksConfirmedDelegate(IList selectedTracks);

        public event TracksConfirmedDelegate TracksConfirmed;

        private readonly NavigationService navigationService;

        private readonly ISnackbarService _snackbarService;

        private readonly VkService vkService;

        private readonly Logger logger;

        private bool allLoaded = false;

        public TracksSelectorModalViewModel(NavigationService navigationService, VkService vkService, Logger logger,
            ISnackbarService snackbarService)
        {
            this.navigationService = navigationService;
            this.vkService = vkService;
            _snackbarService = snackbarService;
            this.navigationService = navigationService;
            this.logger = logger;
            this.ConfirmCommand = new RelayCommand(Confirm);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        private void Confirm()
        {
            navigationService.CloseModal();

            this.TracksConfirmed?.Invoke(SelectedTracks);

            SelectedTracks?.Clear();
        }

        private void Cancel()
        {
            SelectedTracks?.Clear();

            this.Confirm();
        }

        public async Task LoadTracksAsync()
        {
            try
            {
                if (allLoaded) return;
                var tracks = await this.GetTracksAsync(20);

                allLoaded = tracks.Count == 0;
                foreach (var track in tracks)
                {
                    Tracks.Add(track);
                }

                IsLoading = false;
            }catch(Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);

                _snackbarService.ShowException("Ошибка", "Music X не смог загрузить список Ваших треков. Попробуйте ещё раз");
            }
           
        }


        private async Task<List<Audio>> GetTracksAsync(int count)
        {
            var tracks = await vkService.AudioGetAsync(null, null, null, Tracks.Count, count);

            return tracks.Items;

        }

    }
}
