using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Crashes;
using Microsoft.Win32;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using MusicX.Views.Modals;
using System;
using System.Collections.Generic;
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

        public ICommand AddTracksCommand { get; }

        public ICommand OpenCoverPathCommand { get; set; }

        public ICommand DeleteTrackCommand { get; set; }

        public event Action<bool> EndEvent;

        private long editPlaylistId { get; set; }

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

            this.AddTracksCommand = new RelayCommand(AddTracks);
            this.CreateCommand = new AsyncCommand(Create);
            this.OpenCoverPathCommand = new AsyncCommand(OpenCoverPath);
            this.DeleteTrackCommand = new RelayCommand((parameter) => DeleteTrack(parameter));
            this.selectorViewModel.TracksConfirmed += AddSelectedTracks;    
            
        }

        public void LoadDataFromPlaylist(Playlist plist)
        {
            this.CreateIsEnable = true;
            this.CoverPath = plist.Cover;
            this.Description = plist.Description;
            this.HideFromSearch = true;
            this.IsEdit = true;
            this.Title = plist.Title;
            this.editPlaylistId = plist.Id;

            foreach (var track in plist.Audios)
            {
                this.Tracks.Add(track);
            }

        }

        private void DeleteTrack(object parameter)
        {
            var track = parameter as Audio;

            Tracks.Remove(track);
        }

        private void AddTracks()
        {
            navigationService.OpenModal<TracksSelectorModal>(selectorViewModel);
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

                var config = await configService.GetConfig();

                long playlistId = 0;

                if(isEdit)
                {
                    await EditAsync(config, this.editPlaylistId);
                    playlistId = editPlaylistId;
                }else
                {
                    playlistId = await vkService.CreatePlaylistAsync(config.UserId, this.Title, this.Description, Tracks.ToList());

                }

                if(!this.CoverPath.StartsWith("http"))
                {
                    await UploadCoverPlaylist(config, playlistId);
                }

                Tracks.Clear();

                navigationService.CloseModal();

                if(isEdit)
                {
                    notificationsService.Show("Плейлист изменен", $"Плейлист '{Title}' теперь изменен.");

                }else
                {
                    notificationsService.Show("Плейлист создан", $"Плейлист '{Title}' теперь находится в списке Ваших плейлистов.");
                }

                EndEvent?.Invoke(true);

            }
            catch(Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                CreateIsEnable = true;

                notificationsService.Show("Ошибка", $"MusicX не смог создать плейлист :(");

                EndEvent?.Invoke(false);


            }

        }

        private async Task UploadCoverPlaylist(ConfigModel config, long playlistId)
        {
            if (!string.IsNullOrEmpty(CoverPath))
            {
                var uploadServer = await vkService.GetPlaylistCoverUploadServerAsync(config.UserId, playlistId);

                var image = await vkService.UploadPlaylistCoverAsync(uploadServer.Response.UploadUrl, CoverPath);

                await vkService.SetPlaylistCoverAsync(config.UserId, playlistId, image.Hash, image.Photo);
            }
        }

        private async Task EditAsync(ConfigModel config, long playlistId)
        {
            try
            {

                await vkService.EditPlaylistAsync(config.UserId, Convert.ToInt32(playlistId), this.Title, this.Description, Tracks.ToList());
            }
            catch(Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                CreateIsEnable = true;

                notificationsService.Show("Ошибка", $"MusicX не смог изменить плейлист :(");
                throw ex;
            }
        }

        private async Task OpenCoverPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения ( *.jpg)|*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                CoverPath = openFileDialog.FileName;
            }
        }

        private void AddSelectedTracks(System.Collections.IList selectedTracks)
        {
            CreateIsEnable = true;

            navigationService.OpenModal<CreatePlaylistModal>(this);

            if(selectedTracks != null)
            {
                foreach (var track in selectedTracks) //очень надеюсь что какой то еблан не выберет 100 треков и не будет ждать пол часа добавления блять.
                {
                    Tracks.Add(track as Audio);
                }
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
