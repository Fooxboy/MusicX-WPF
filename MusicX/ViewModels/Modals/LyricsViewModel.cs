using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.AppCenter.Crashes;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Shared.Player;
using NLog;
using Wpf.Ui.Contracts;

namespace MusicX.ViewModels.Modals
{
    public class LyricsViewModel : BaseViewModel
    {
        private readonly VkService _vkService;
        private readonly ISnackbarService _snackbarService;
        private readonly PlayerService _playerService;

        private readonly Logger _logger;

        public bool IsLoading { get; set; }

        public string Credits { get; set; }

        public List<LyricsTimestamp> Timestamps { get; set; }

        public List<string> Texts { get; set; }

        public event Action<int> NextLineEvent;

        public event Action NewTrack;

        public PlaylistTrack Track { get; set; }

        private DispatcherTimer _timer { get; set; }

        public LyricsViewModel(VkService vkService, ISnackbarService snackbarService, PlayerService playerService,
            Logger logger)
        {
            this._vkService = vkService;
            _snackbarService = snackbarService;
            this._playerService = playerService;
            this._logger = logger;

            playerService.TrackChangedEvent += PlayerService_TrackChangedEvent;
        }

        private void PlayerService_TrackChangedEvent(object? sender, EventArgs e)
        {
            Track = _playerService.CurrentTrack;
            NewTrack?.Invoke();
        }

        public async Task LoadLyrics()
        {
            try
            {
                _logger.Info("Загрузка текста песни");
                if (Track.Data is VkTrackData track)
                {
                    if(track.HasLyrics == null || !track.HasLyrics.Value)
                    {
                        Texts = new List<string>() { "Этот трек", "Не имеет текста" };
                        IsLoading = false;
                        OnPropertyChanged(nameof(IsLoading));

                        return;

                    }

                    IsLoading = true;
                    OnPropertyChanged(nameof(IsLoading));
                    var vkLyrics = await _vkService.GetLyrics(track.Info.OwnerId + "_" + track.Info.Id);

                    Timestamps = vkLyrics.LyricsInfo.Timestamps;

                    if(Timestamps is null)
                    {
                        Texts = vkLyrics.LyricsInfo.Text;
                    }

                    if(_timer is null)
                    {
                        _timer = new DispatcherTimer();
                        _timer.Interval = TimeSpan.FromMilliseconds(500);
                        _timer.Tick += Timer_Tick;
                        _timer.Start();
                    }
                 

                    IsLoading = false;
                    OnPropertyChanged(nameof(IsLoading));

                    Credits = vkLyrics.Credits;
                }
                else
                {
                    _logger.Info("Текст песни из бума не поддерживается");

                    _snackbarService.Show("Ошибка", "Треки из миксов не поддерживаются.");
                }
            }
            catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                _logger.Error(ex.Message, ex);
                _snackbarService.Show("Ошибка", "Мы не смогли загрузить текст песни :(");
            }
            
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var currentPositionOnMs = _playerService.Position.TotalMilliseconds;

                NextLineEvent?.Invoke(Convert.ToInt32(currentPositionOnMs));

            }
            catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
            }
        }
    }
}
