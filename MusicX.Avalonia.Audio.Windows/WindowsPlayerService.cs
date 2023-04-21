using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage.Streams;
using MusicX.Avalonia.Audio.Services;
using MusicX.Avalonia.Core.Extensions;
using MusicX.Shared.Player;

namespace MusicX.Avalonia.Audio.Windows;

public sealed class WindowsPlayerService : IPlayerService
{
    public MediaPlayer Player { get; } = new()
    {
        AudioCategory = MediaPlayerAudioCategory.Media,
        SystemMediaTransportControls =
        {
            DisplayUpdater =
            {
                AppMediaId = "MusicX"
            }
        }
    };
    
    private PlaylistTrack? _currentTrack;

    public WindowsPlayerService()
    {
        
        Player.MediaEnded += PlayerOnMediaEnded;
        Player.CurrentStateChanged += PlayerOnCurrentStateChanged;
    }

    private void PlayerOnCurrentStateChanged(MediaPlayer sender, object args)
    {
        if (CurrentTrack is not null)
            OnPropertyChanged(nameof(IsPlaying));
    }

    private void PlayerOnMediaEnded(MediaPlayer sender, object args)
    {
        if (Repeat || CurrentTrack is null) return;
        
        TrackEnded?.Invoke(this, EventArgs.Empty);
        Stop();
    }

    public void Dispose()
    {
        Player.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public PlaylistTrack? CurrentTrack
    {
        get => _currentTrack;
        private set
        {
            if (Equals(value, _currentTrack)) return;
            _currentTrack = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan Elapsed
    {
        get => Player.Position;
        set
        {
            if (Equals(value, Player.Position)) return;
            Player.Position = value;
            OnPropertyChanged();
        }
    }

    public double Volume
    {
        get => Player.Volume;
        set
        {
            if (Equals(value, Player.Volume)) return;
            Player.Volume = value;
            OnPropertyChanged();
        }
    }

    public bool Repeat
    {
        get => Player.IsLoopingEnabled;
        set
        {
            if (Equals(value, Player.IsLoopingEnabled)) return;
            Player.IsLoopingEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsPlaying
    {
        get => Player.CurrentState is MediaPlayerState.Playing or MediaPlayerState.Opening or MediaPlayerState.Buffering;
        set => throw new NotSupportedException();
    }

    public event EventHandler? TrackEnded;
    public async void Play(PlaylistTrack track, bool resetExisting = false)
    {
        if (Player.Source is not null && CurrentTrack == track)
        {
            Player.Play();
            OnPropertyChanged(nameof(IsPlaying));
            return;
        }

        CurrentTrack = track;

        var hlsResult = await AdaptiveMediaSource.CreateFromUriAsync(new(track.Data.Url));
        var mediaSource = MediaSource.CreateFromAdaptiveMediaSource(hlsResult.MediaSource);
        Player.Source = new MediaPlaybackItem(mediaSource);
        
        Player.Play();
        OnPropertyChanged(nameof(IsPlaying));

        await Task.Delay(TimeSpan.FromSeconds(1));

        if (!IsPlaying || CurrentTrack is null)
            return;
        
        PushTrackData();
    }

    private void PushTrackData()
    {
        var updater = Player.SystemMediaTransportControls.DisplayUpdater;
        updater.MusicProperties.Title = CurrentTrack!.Title;
        updater.MusicProperties.Artist = CurrentTrack.GetArtistsString();
        updater.MusicProperties.TrackNumber = 1;
        updater.MusicProperties.AlbumArtist = CurrentTrack.MainArtists[0].Name;
        updater.MusicProperties.AlbumTitle = CurrentTrack.AlbumId?.Name ?? string.Empty;
        updater.MusicProperties.AlbumTrackCount = 1;
        
        if (CurrentTrack.AlbumId is not null)
            Player.SystemMediaTransportControls.DisplayUpdater.Thumbnail =
                RandomAccessStreamReference.CreateFromUri(new Uri(CurrentTrack.AlbumId.CoverUrl));
        Player.SystemMediaTransportControls.DisplayUpdater.Update();
    }

    public void Pause()
    {
        Player.Pause();
        OnPropertyChanged(nameof(IsPlaying));
    }

    public void Stop()
    {
        CurrentTrack = null;
        OnPropertyChanged(nameof(IsPlaying));

        if (IsPlaying)
            Player.Pause();
        Player.Source = null;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}