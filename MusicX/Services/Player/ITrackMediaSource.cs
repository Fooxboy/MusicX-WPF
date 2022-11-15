using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;

namespace MusicX.Services.Player;

public interface ITrackMediaSource
{
    Task<MediaSource?> CreateMediaSourceAsync(PlaylistTrack track, CancellationToken cancellationToken = default);
}