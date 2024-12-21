using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services.Player;
using MusicX.Shared.Player;
using NLog;
using Wpf.Ui;

namespace MusicX.ViewModels.Modals
{
    public class LyricsViewModel : BaseViewModel
    {
        private readonly VkService _vkService;
        private readonly ISnackbarService _snackbarService;
        private readonly PlayerService _playerService;

        private readonly Logger _logger;
        private readonly GeniusService _geniusService;

        public bool IsLoading { get; set; }

        public string Credits { get; set; }

        public List<LyricsTimestamp> Timestamps { get; set; }

        public List<string> Texts { get; set; }

        public event Action<int> NextLineEvent;

        public event Action NewTrack;

        public PlaylistTrack Track { get; set; }

        private DispatcherTimer _timer { get; set; }

        public LyricsViewModel(VkService vkService, ISnackbarService snackbarService, PlayerService playerService,
            Logger logger, GeniusService geniusService)
        {
            this._vkService = vkService;
            _snackbarService = snackbarService;
            this._playerService = playerService;
            this._logger = logger;
            _geniusService = geniusService;
            playerService.TrackChangedEvent += PlayerService_TrackChangedEvent;
        }

        private void PlayerService_TrackChangedEvent(object? sender, EventArgs e)
        {
            Track = _playerService.CurrentTrack!;
            NewTrack?.Invoke();
        }

        public async Task LoadLyrics(bool isGenius)
        {
            try
            {
                IsLoading = true;
                _logger.Info("Загрузка текста песни");
                if (Track.Data is not VkTrackData track)
                {
                    await LoadGenius();
                    IsLoading = false;
                    return;
                }

                var result = isGenius ? await LoadGenius() : await LoadLyricFind(track);
                if (!result)
                    return;

                if (_timer is null)
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromMilliseconds(500);
                    _timer.Tick += Timer_Tick;
                    _timer.Start();
                }

                IsLoading = false;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to load track lyrics");
                _snackbarService.ShowException("Ошибка", "Мы не смогли загрузить текст песни :(");

                Texts = new List<string>() { "Ошибка загрузки" };
                IsLoading = false;
            }
        }

        private async Task<bool> LoadLyricFind(VkTrackData track)
        {
            if (track.HasLyrics == null || !track.HasLyrics.Value)
            {
                Texts = new List<string>() { "Этот трек", "Не имеет текста" };
                IsLoading = false;

                return false;

            }

            var vkLyrics = await _vkService.GetLyrics(track.Info.OwnerId + "_" + track.Info.Id);

            Timestamps = vkLyrics.LyricsInfo.Timestamps;

            if (Timestamps is null)
            {
                Texts = vkLyrics.LyricsInfo.Text;
            }

            Credits = vkLyrics.Credits;

            return true;
        }

        private async Task<bool> LoadGenius()
        {
            var hits = await _geniusService.SearchAsync($"{Track.Title} {Track.MainArtists.First().Name}");

            if (!hits.Any())
            {
                Texts = new List<string>() { "Этот трек", "Не имеет текста" };
                IsLoading = false;

                return false;
            }

            var song = await _geniusService.GetSongAsync(hits.First().Result.Id);

            Texts = song.Lyrics.Plain.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            Credits = "Текст предоставлен Genius (genius.com).\nВКонтакте и MusicX к данному сервису, а также к содержанию текста отношения не имеют.";

            return true;
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
                _logger.Error(ex, "Failed to jump to next lyrics line");
            }
        }
    }
}
