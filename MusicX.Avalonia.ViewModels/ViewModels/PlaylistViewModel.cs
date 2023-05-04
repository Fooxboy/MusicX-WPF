using System.Reactive;
using DynamicData;
using DynamicData.Binding;
using MusicX.Avalonia.Audio.Playlists;
using MusicX.Avalonia.Audio.Services;
using MusicX.Avalonia.Core.Extensions;
using MusicX.Avalonia.Core.Models;
using MusicX.Avalonia.Core.Services;
using MusicX.Shared.Player;
using ReactiveUI;
using VkApi;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class PlaylistViewModel : ViewModelBase
{
    private readonly Api _api;
    private readonly GlobalViewModel _viewModel;
    private readonly ConfigurationService _configurationService;
    private readonly IQueueService _queueService;

    public bool IsLoading { get; set; }
    public ReactiveCommand<PlaylistTrack?,Unit> PlayTrackCommand { get; }

    public PlaylistViewModel(IPlayerService playerService, Api api, GlobalViewModel viewModel, ConfigurationService configurationService, IQueueService queueService)
    {
        _api = api;
        _viewModel = viewModel;
        _configurationService = configurationService;
        _queueService = queueService;
        PlayerService = playerService;
        Tracks.ObserveCollectionChanges().Subscribe(_ =>
        {
            var total = Tracks.Select(b => b.Data.Duration)
                              .Aggregate((a, b) => a + b);
            AudiosTotalLengthFormatted =
                string.Format(total.TotalDays > 2 ? 
                                  "{0:%d} дня {0:%h} часов {0:%m} минут" : 
                                  "{0:%h} часов {0:%m} минут", 
                              total);
        });

        PlayTrackCommand = ReactiveCommand.CreateFromTask<PlaylistTrack?>(PlayTrackAsync);
    }

    private Task PlayTrackAsync(PlaylistTrack? track)
    {
        return _queueService.PlayPlaylistAsync(new PlaylistPlaylist(_api, Playlist, Tracks), CancellationToken.None, track).AsTask();
    }

    public PlaylistOwner Owner { get; set; } = null!;
    public CatalogPlaylist Playlist { get; set; } = null!;
    public string AudiosTotalLengthFormatted { get; set; } = null!;

    public IObservableCollection<PlaylistTrack> Tracks { get; set; } =
        new ObservableCollectionExtended<PlaylistTrack>();

    public IPlayerService PlayerService { get; }

    public async Task LoadAsync(CatalogPlaylist playlist)
    {
        Playlist = playlist;
        IsLoading = true;
        var audios = await _api.GetAudioAsync(new((int?)Playlist.OwnerId, null, Playlist.Id, null, true, null, null,
                                                     100, null, null, Playlist.AccessKey, null, null, null));

        Tracks.AddRange(audios.Items.Select(TrackExtensions.ToTrack));
        
        if (Playlist.MainArtists?.Count > 0)
        {
            var artist = Playlist.MainArtists.First();
            Owner = new(artist.Id, artist.Name);
        }
        else if (Playlist.OwnerName is not null)
        {
            Owner = new(Playlist.OwnerId.ToString(), Playlist.OwnerName);
        }
        else if (Playlist.OwnerId == _configurationService.LoginState!.UserId)
        {
            Owner = new(Playlist.OwnerId.ToString(), "ВКонтакте");
        }
        else
        {
            var profileOrGroup = await _viewModel.GetProfileAsync(Playlist.OwnerId);
            Owner = new(Playlist.OwnerId.ToString(), profileOrGroup.IsT0 ? $"{profileOrGroup.AsT0.FirstName} {profileOrGroup.AsT0.LastName}" : profileOrGroup.AsT1.Name);
        }
        
        IsLoading = false;
    }

    public async Task LoadMoreAsync()
    {
        IsLoading = true;
        var audios = await _api.GetAudioAsync(new((int?)Playlist.OwnerId, null, Playlist.Id, null, true, null, Tracks.Count,
                                                  50, null, null, Playlist.AccessKey, null, null, null));

        Tracks.AddRange(audios.Items.Select(TrackExtensions.ToTrack));
        IsLoading = false;
    }
}

public record PlaylistOwner(string Id, string Name);