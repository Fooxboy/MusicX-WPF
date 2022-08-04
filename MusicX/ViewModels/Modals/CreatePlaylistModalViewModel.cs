using AsyncAwaitBestPractices.MVVM;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views.Modals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPFUI.Common;

namespace MusicX.ViewModels.Modals
{
    public class CreatePlaylistModalViewModel : BaseViewModel
    {
        public string CoverPath { get; set; }

        public bool HideFromSearch { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ObservableCollection<Audio> Tracks { get; } = new ObservableCollection<Audio>();

        public ICommand CreateCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand AddTracksCommand { get; }

        public ICommand OpenCoverPathCommand { get; set; }

        private readonly NavigationService navigationService;

        private readonly VkService vkService;

        private readonly TracksSelectorModalViewModel selectorViewModel;

        public CreatePlaylistModalViewModel(NavigationService navigationService, VkService vkService, TracksSelectorModalViewModel selectorViewModel)
        {
            this.navigationService = navigationService;
            this.vkService = vkService;
            this.selectorViewModel = selectorViewModel;

            this.CancelCommand = new RelayCommand(Cancel);
            this.AddTracksCommand = new RelayCommand(AddTracks);
            this.CreateCommand = new AsyncCommand(Create);
            this.OpenCoverPathCommand = new AsyncCommand(OpenCoverPath);

            this.selectorViewModel.TracksConfirmed += AddSelectedTracks;

            this.Tracks.Add(new Audio() { Title = "блядина", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "блядина", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "блядина", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "блядина", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "блядина", Artist = "да наверное", IsExplicit = true, });
            this.Tracks.Add(new Audio() { Title = "блядина", Artist = "да наверное", IsExplicit = true, });
        }

        private void Cancel()
        {
            navigationService.CloseModal();
        }

        private void AddTracks()
        {
            var modal = new TracksSelectorModal(selectorViewModel);

            navigationService.OpenModal(modal, 700, 600);
        }

        private async Task Create()
        {
            
        }

        private async Task OpenCoverPath()
        {

        }

        private void AddSelectedTracks(List<Audio> selectedTracks)
        {
            navigationService.OpenModal(new CreatePlaylistModal(this), 700, 600);

            foreach (var track in selectedTracks) //очень надеюсь что какой то еблан не выберет 100 треков и не будет ждать пол часа добавления блять.
            {
                Tracks.Add(track);
            }
        }
    }
}
