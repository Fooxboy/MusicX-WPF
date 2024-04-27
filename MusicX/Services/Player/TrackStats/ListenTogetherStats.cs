using MusicX.Models.Enums;
using MusicX.Shared.Player;
using System;
using System.Threading.Tasks;
using MusicX.Core.Models;
using MusicX.Core.Services;

namespace MusicX.Services.Player.TrackStats
{
    public class ListenTogetherStats : ITrackStatsListener
    {
        private readonly ListenTogetherService _listenTogetherService;
        public ListenTogetherStats(ListenTogetherService listenTogetherService)
        {
            this._listenTogetherService = listenTogetherService;
        }

        public async Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason, TimeSpan? position = null)
        {
            if (_listenTogetherService.PlayerMode != PlayerMode.Owner) return;

            await _listenTogetherService.ChangeTrackAsync(newTrack);

        }

        public async Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused)
        {
            if (_listenTogetherService.PlayerMode != PlayerMode.Owner) return;

            await _listenTogetherService.ChangePlayStateAsync(position, paused);
        }
    }
}
