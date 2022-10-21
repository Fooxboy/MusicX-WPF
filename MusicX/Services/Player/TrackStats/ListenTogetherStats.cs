using MusicX.Helpers;
using MusicX.Models.Enums;
using MusicX.Shared.Player;
using System;
using System.Threading.Tasks;

namespace MusicX.Services.Player.TrackStats
{
    public class ListenTogetherStats : ITrackStatsListener
    {
        private readonly ListenTogetherService _listenTogetherService;
        public ListenTogetherStats(ListenTogetherService listenTogetherService)
        {
            this._listenTogetherService = listenTogetherService;
        }

        public async Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason)
        {
            if (_listenTogetherService.PlayerMode != PlayerMode.Owner) return;

            var track = newTrack.ToRemoteTrack();

            await _listenTogetherService.ChangeTrackAsync(track);

        }

        public async Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused)
        {
            if (_listenTogetherService.PlayerMode != PlayerMode.Owner) return;

            await _listenTogetherService.ChangePlayStateAsync(position, paused);
        }
    }
}
