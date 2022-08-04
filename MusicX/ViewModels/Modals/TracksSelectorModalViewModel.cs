using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Views.Modals;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WPFUI.Common;

namespace MusicX.ViewModels.Modals
{
    public class TracksSelectorModalViewModel
    {
        public List<Audio> SelectedTracks { get; set; } = new List<Audio>();

        public ObservableCollection<Audio> Tracks { get; } = new ObservableCollection<Audio>();

        public ICommand ConfirmCommand { get; }
        
        public ICommand CancelCommand { get; }

        public delegate void TracksConfirmedDelegate(List<Audio> selectedTracks);

        public event TracksConfirmedDelegate TracksConfirmed;

        private readonly NavigationService navigationService;

        public TracksSelectorModalViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.ConfirmCommand = new RelayCommand(Confirm);
            this.CancelCommand = new RelayCommand(Cancel);

            this.Tracks.Add(new Audio() { Title = "имя трека", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "имя трека", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "имя трека", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "имя трека", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "имя трека", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "имя трека", Artist = "да наверное", IsExplicit = true, });
        }

        private void Confirm()
        {
            navigationService.CloseModal();

            this.TracksConfirmed?.Invoke(SelectedTracks);
        }

        private void Cancel()
        {
            SelectedTracks.Clear();

            this.Confirm();
        }

    }
}
