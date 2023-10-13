using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Models.Enums;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.TrackStats;

public class VkTrackStats : ITrackStatsListener
{
    private readonly VkService _vkService;

    public VkTrackStats(VkService vkService)
    {
        _vkService = vkService;
    }

    public async Task TrackChangedAsync(PlaylistTrack? previousTrack, PlaylistTrack newTrack, ChangeReason reason)
    {
        if (newTrack.Data is not VkTrackData newTrackData)
            return;

        var startPlay = new PlayTrackEvent
        {
            Event = "music_start_playback",
            AudioId = newTrackData.Info.ToOwnerIdString(),
            Uuid = Guid.NewGuid().GetHashCode(),
            StartTime = "0",
            Shuffle = "false",
            Reason = "auto",
            PlaybackStartedAt = "0",
            TrackCode = newTrackData.TrackCode,
            Repeat = "all",
            State = "app",
            Source = newTrackData.ParentBlockId!,
            PlaylistId = newTrackData.Playlist?.ToOwnerIdString()!
        };

        var queue = new List<TrackEvent>(2) { startPlay };

        if (previousTrack?.Data is VkTrackData previousTrackData)
        {
            startPlay.PrevAudioId = previousTrackData.Info.ToOwnerIdString();

            queue.Add(new StopTrackEvent
            {
                Event = "music_stop_playback",
                Uuid = Guid.NewGuid().GetHashCode(),
                Shuffle = "false",
                Reason = "new",
                AudioId = previousTrackData.Info.ToOwnerIdString(),
                StartTime = "0",
                PlaybackStartedAt = "0",
                TrackCode = previousTrackData.TrackCode,
                StreamingType = "online",
                Duration = previousTrackData.Duration.TotalSeconds.ToString(),
                Repeat = "all",
                State = "app",
                Source = newTrackData.ParentBlockId!,
                PlaylistId = newTrackData.Playlist?.ToOwnerIdString()!
            });
        }

        await _vkService.StatsTrackEvents(queue);
    }

    public Task TrackPlayStateChangedAsync(PlaylistTrack track, TimeSpan position, bool paused)
    {
        return Task.CompletedTask;
    }
}