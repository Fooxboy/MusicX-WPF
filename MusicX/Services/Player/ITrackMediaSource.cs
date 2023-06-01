using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using MusicX.Shared.Player;

namespace MusicX.Services.Player;

public interface ITrackMediaSource
{
    Task<MediaPlaybackItem?> CreateMediaSourceAsync(PlaylistTrack track, CancellationToken cancellationToken = default);
}