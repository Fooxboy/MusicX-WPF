using System.ComponentModel;
using MusicX.Shared.Player;

namespace MusicX.Avalonia.Audio.Services;

public interface IPlayerService : IDisposable, INotifyPropertyChanged
{
    PlaylistTrack? CurrentTrack { get; }
    TimeSpan Elapsed { get; set; }
    double Volume { get; set; }
    bool Repeat { get; set; }
    bool IsPlaying { get; set; }
    event EventHandler? TrackEnded;
    void Play(PlaylistTrack track, bool resetExisting = false);
    void Pause();
    void Stop();
}