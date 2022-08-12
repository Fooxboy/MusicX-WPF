using AsyncAwaitBestPractices.MVVM;
using Microsoft.Win32;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views.Modals;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Common;

namespace MusicX.ViewModels.Modals
{
    public class CreatePlaylistModalViewModel : BaseViewModel
    {
        public string CoverPath { get; set; } = "";

        public bool HideFromSearch { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ObservableCollection<Audio> Tracks { get; set; } = new ObservableCollection<Audio>();

        public bool CreateIsEnable { get; set; } = false;

        public ICommand CreateCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand AddTracksCommand { get; }

        public ICommand OpenCoverPathCommand { get; set; }

        private readonly NavigationService navigationService;

        private readonly VkService vkService;

        private readonly TracksSelectorModalViewModel selectorViewModel;
        private readonly ConfigService configService;
        private readonly NotificationsService notificationsService;

        private bool isEdit;
        public bool IsEdit
        {
            get => isEdit;
            set
            {
                if(!value) ClearData();
                isEdit = value;
            }
        }

        public CreatePlaylistModalViewModel(NavigationService navigationService, VkService vkService, TracksSelectorModalViewModel selectorViewModel, ConfigService configService, NotificationsService notificationsService)
        {
            this.navigationService = navigationService;
            this.vkService = vkService;
            this.selectorViewModel = selectorViewModel;
            this.configService = configService;
            this.notificationsService = notificationsService;

            this.CancelCommand = new RelayCommand(Cancel);
            this.AddTracksCommand = new RelayCommand(AddTracks);
            this.CreateCommand = new AsyncCommand(Create);
            this.OpenCoverPathCommand = new AsyncCommand(OpenCoverPath);

            this.selectorViewModel.TracksConfirmed += AddSelectedTracks;       
        }

        private void Cancel()
        {
            Tracks.Clear();
            navigationService.CloseModal();
        }

        private void AddTracks()
        {
            var modal = new TracksSelectorModal(selectorViewModel);

            navigationService.OpenModal(modal, 700, 600);
        }

        private async Task Create()
        {
            try
            {
                if(string.IsNullOrEmpty(Title) || string.IsNullOrWhiteSpace(Title))
                {
                    notificationsService.Show("Обязательные поля не заполнены", "Вы должны заполнить название плейлиста");
                    return;
                }

                CreateIsEnable = false;
                Changed(nameof(CreateIsEnable));

                var config = await configService.GetConfig();

                var playlistId = await vkService.CreatePlaylistAsync(config.UserId, this.Title, this.Description, Tracks.ToList());

                if(!string.IsNullOrEmpty(CoverPath))
                {
                    var uploadServer = await vkService.GetPlaylistCoverUploadServerAsync(config.UserId, playlistId);

                    var image = await vkService.UploadPlaylistCoverAsync(uploadServer.Response.UploadUrl, CoverPath);

                    await vkService.SetPlaylistCoverAsync(config.UserId, playlistId, image.Hash, image.Photo);
                }
               
                Tracks.Clear();

                navigationService.CloseModal();

                notificationsService.Show("Плейлист создан", $"Плейлист '{Title}' теперь находится в списке Ваших плейлистов.");

            }catch(Exception ex)
            {
                CreateIsEnable = true;
                Changed(nameof(CreateIsEnable));

                notificationsService.Show("Ошибка", $"MusicX не смог создать плейлист :(");

            }

        }

        private async Task OpenCoverPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения ( *.jpg)|*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                CoverPath = openFileDialog.FileName;
                Changed("CoverPath");
            }
        }

        private void AddSelectedTracks(System.Collections.IList selectedTracks)
        {
            CreateIsEnable = true;
            Changed(nameof(CreateIsEnable));

            navigationService.OpenModal(new CreatePlaylistModal(this), 700, 600);

            if(selectedTracks != null)
            {
                foreach (var track in selectedTracks) //очень надеюсь что какой то еблан не выберет 100 треков и не будет ждать пол часа добавления блять.
                {
                    Tracks.Add(track as Audio);
                }

                Changed("Tracks");
            }
        }

        public void MoveTracks(Audio source, Audio target)
        {
            var removedIdx = Tracks.IndexOf(source);
            var targetIdx = Tracks.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                Tracks.Insert(targetIdx + 1, source);
                Tracks.RemoveAt(removedIdx);
            }
            else
            {
                if (Tracks.Count + 1 <= ++removedIdx)
                    return;
                Tracks.Insert(targetIdx, source);
                Tracks.RemoveAt(removedIdx);
            }
        }

        private void ClearData()
        {
            CoverPath = "";
            Title = "";
            Description = "";
            Tracks.Clear();
        }
    }
}
