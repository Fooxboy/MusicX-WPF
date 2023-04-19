using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive;
using System.Runtime.InteropServices;
using MusicX.Avalonia.Audio.Playlists;
using MusicX.Avalonia.Audio.Services;
using MusicX.Avalonia.Core.Extensions;
using MusicX.Avalonia.Core.Models;
using MusicX.Shared.Player;
using OneOf;
using ReactiveUI;
using VkApi;

namespace MusicX.Avalonia.ViewModels.ViewModels;

public class GlobalViewModel : ViewModelBase
{
    private readonly ConcurrentDictionary<long, OneOf<CatalogProfile, CatalogGroup>> _profiles = new();
    private readonly QueueService _queueService;
    private readonly Api _api;
    private readonly IServiceProvider _provider;
    public PlayerService PlayerService { get; }
    public ReactiveCommand<CatalogAudio, Unit> AudioClickCommand { get; }
    public ReactiveCommand<CatalogPlaylist, Unit> OpenPlaylistCommand { get; }
    public ReactiveCommand<PlaylistTrack,Unit> TrackClickCommand { get; }
    public ReactiveCommand<CatalogMainArtist,Unit> OpenArtistCommand { get; }
    public ReactiveCommand<string,Unit> OpenSectionCommand { get; }
    public ReactiveCommand<CatalogLink,Unit> OpenLinkCommand { get; }
    public ReactiveCommand<CatalogVideo,Unit> OpenVideoPlayerCommand { get; }
    public ReactiveCommand<CatalogAction,Unit> OpenActionCommand { get; }

    public GlobalViewModel(PlayerService playerService, QueueService queueService, Api api, IServiceProvider provider)
    {
        _queueService = queueService;
        _api = api;
        _provider = provider;
        PlayerService = playerService;
        AudioClickCommand = ReactiveCommand.CreateFromTask<CatalogAudio>(PlayAsync);
        TrackClickCommand = ReactiveCommand.CreateFromTask<PlaylistTrack>(PlayAsync);
        OpenPlaylistCommand = ReactiveCommand.CreateFromTask<CatalogPlaylist>(OpenPlaylistAsync);
        OpenArtistCommand = ReactiveCommand.CreateFromTask<CatalogMainArtist>(OpenArtistAsync);
        OpenSectionCommand = ReactiveCommand.CreateFromTask<string>(OpenSectionAsync);
        OpenLinkCommand = ReactiveCommand.CreateFromTask<CatalogLink>(OpenLinkAsync);
        OpenVideoPlayerCommand = ReactiveCommand.CreateFromTask<CatalogVideo>(OpenVideoAsync);
        OpenActionCommand = ReactiveCommand.CreateFromTask<CatalogAction>(OpenActionAsync);
    }

    private Task OpenActionAsync(CatalogAction action)
    {
        if (!string.IsNullOrEmpty(action.SectionId))
            return OpenSectionAsync(action.SectionId);
        
        if (!string.IsNullOrEmpty(action.Action.Url))
            OpenUrl(action.Action.Url);
        
        return Task.CompletedTask;
    }

    private async Task OpenVideoAsync(CatalogVideo video)
    {
        // TODO WebView2 player when someone gets working glue for avalonia v11
        // var viewModel = (VideoModalViewModel)_provider.GetService(typeof(VideoModalViewModel))!;
        //
        // await viewModel.LoadVideoAsync(video);
        // MessageBus.Current.SendMessage<ViewModelBase>(viewModel, "nav");

        OpenUrl(video.Player);
    }

    private static void OpenUrl(string url)
    {
        if (OperatingSystem.IsWindows())
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        else
        {
            Process.Start("open", url);
        }
    }

    private async Task OpenLinkAsync(CatalogLink link)
    {
        switch (link.Meta.ContentType)
        {
            case "user" or "group" or null:
            {
                var catalog = await _api.GetCatalogAudioAsync(new(null, null, link.Url, null));
                await OpenSectionAsync(catalog.Catalog.DefaultSection);
                break;
            }
            case "artist":
            {
                var uri = new Uri(link.Url);
                var artist =
                    await _api.GetCatalogAudioArtistAsync(new(uri.Segments.Last(), null, null, null,
                                                              link.Meta.TrackCode));
                await OpenSectionAsync(artist.Catalog.DefaultSection);
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException(nameof(link.Meta.ContentType), link.Meta.ContentType,
                                                      "Unknown content type");
        }
    }

    private async Task OpenArtistAsync(CatalogMainArtist artist)
    {
        var catalog = await _api.GetCatalogAudioArtistAsync(new(artist.Id, null, null, null, null));
        await OpenSectionAsync(catalog.Catalog.DefaultSection);
    }

    public async Task OpenSectionAsync(string id)
    {
        var response = await _api.GetCatalogSectionAsync(new(id, null, null, null, null, null, null, null, null));
        var viewModel = (SectionTabViewModel)_provider.GetService(typeof(SectionTabViewModel))!;

        viewModel.Init(id, response.Section.Title);
        if (response.Profiles is not null) CacheProfiles(response.Profiles);
        if (response.Groups is not null) CacheProfiles(response.Groups);
        MessageBus.Current.SendMessage<ViewModelBase>(viewModel, "nav");
    }

    public async Task OpenPlaylistAsync(CatalogPlaylist arg)
    {
        var viewModel = (PlaylistViewModel)_provider.GetService(typeof(PlaylistViewModel))!;

        await viewModel.LoadAsync(arg);
        
        MessageBus.Current.SendMessage<ViewModelBase>(viewModel, "nav");
    }

    private Task PlayAsync(CatalogAudio audio, CancellationToken token)
    {
        IPlaylist playlist;
        if (audio.ParentBlockId is null)
            playlist = new SinglePlaylist(audio.ToTrack());
        else
            playlist = new BlockPlaylist(_api, audio.ParentBlockId);

        return _queueService.PlayPlaylistAsync(playlist, token, audio.Url).AsTask();
    }

    private Task PlayAsync(PlaylistTrack track, CancellationToken token)
    {
        if (track.Data is not VkTrackData data)
            return Task.CompletedTask;
        
        IPlaylist playlist;
        if (data.ParentBlockId is null)
            playlist = new SinglePlaylist(track);
        else
            playlist = new BlockPlaylist(_api, data.ParentBlockId);

        return _queueService.PlayPlaylistAsync(playlist, token, data.Url).AsTask();
    }

    public void CacheProfiles(IEnumerable<CatalogProfile> profiles)
    {
        foreach (var profile in profiles)
        {
            _profiles.TryAdd(profile.Id, profile);
        }
    }
    
    public void CacheProfiles(IEnumerable<CatalogGroup> profiles)
    {
        foreach (var profile in profiles)
        {
            _profiles.TryAdd(-profile.Id, profile);
        }
    }

    public async ValueTask<OneOf<CatalogProfile, CatalogGroup>> GetProfileAsync(long id)
    {
        if (_profiles.TryGetValue(id, out var profile))
            return profile;

        throw new NotImplementedException();
    }
}