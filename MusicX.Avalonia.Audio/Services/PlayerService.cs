using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ManagedBass;
using MusicX.Shared.Player;

namespace MusicX.Avalonia.Audio.Services;

public class PlayerService : IDisposable, INotifyPropertyChanged
{
    private int? _bassStreamPtr;
    private PlaylistTrack? _currentTrack;
    private bool _repeat;

    public PlaylistTrack? CurrentTrack
    {
        get => _currentTrack;
        private set
        {
            _currentTrack = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan Elapsed
    {
        get => !_bassStreamPtr.HasValue
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(
                Bass.ChannelBytes2Seconds(_bassStreamPtr.Value, Bass.ChannelGetPosition(_bassStreamPtr.Value)));
        set
        {
            var handle = _bassStreamPtr ?? throw new InvalidOperationException("Player is not playing at the moment");
            
            Bass.ChannelSetPosition(handle, Bass.ChannelSeconds2Bytes(handle, value.TotalSeconds));
            
            OnPropertyChanged();
        }
    }

    public double Volume
    {
        get => Bass.Volume;
        set
        {
            Bass.Volume = value;
            OnPropertyChanged();
        }
    }

    public bool Repeat
    {
        get => _repeat;
        set
        {
            _repeat = value;
            OnPropertyChanged();
        }
    }

    public bool IsPlaying => _bassStreamPtr.HasValue && Bass.ChannelIsActive(_bassStreamPtr.Value) == PlaybackState.Playing;

    public event EventHandler? TrackEnded;

    public PlayerService()
    {
        if (!Bass.Init(Frequency: 48000) || Bass.PluginLoad("basshls.dll") == 0)
            throw new ApplicationException("Cannot initialize audio backend");
    }

    public void Play(PlaylistTrack track, bool resetExisting = false)
    {
        if (_bassStreamPtr.HasValue && CurrentTrack == track && Bass.ChannelPlay(_bassStreamPtr.Value, resetExisting))
            return;

        _bassStreamPtr = Bass.CreateStream(track.Data.Url, 0, BassFlags.Float, null);
        ThrowLastError();
        Bass.ChannelSetSync(_bassStreamPtr.Value, SyncFlags.End, 0, EndSync);

        CurrentTrack = track;

        if (Bass.ChannelPlay(_bassStreamPtr.Value, true))
            OnPropertyChanged(nameof(IsPlaying));
    }

    private void EndSync(int handle, int channel, int data, IntPtr user)
    {
        if (CurrentTrack is null) return;
        
        if (Repeat) Play(CurrentTrack, true);
        
        Stop();
        TrackEnded?.Invoke(this, EventArgs.Empty);
    }

    [StackTraceHidden]
    private void ThrowLastError()
    {
        if (Bass.LastError != Errors.OK)
            throw new BassException(Bass.LastError);
    }

    public void Dispose()
    {
        if (_bassStreamPtr.HasValue)
            Stop();
        Bass.Free();
    }

    public void Pause()
    {
        var handle = _bassStreamPtr ?? throw new InvalidOperationException("Player is not playing at the moment");
        if (Bass.ChannelPause(handle))
            OnPropertyChanged(nameof(IsPlaying));
    }

    public void Stop()
    {
        CurrentTrack = null;
        OnPropertyChanged(nameof(IsPlaying));

        if (!_bassStreamPtr.HasValue) return;
        
        Bass.StreamFree(_bassStreamPtr.Value);
        _bassStreamPtr = null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}