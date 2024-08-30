using System.Collections.Immutable;
using System.Collections.Specialized;
using MusicX.Core.Models;
using MusicX.Helpers;
using MusicX.Shared.ListenTogether.Radio;

namespace MusicX.ViewModels;

public class BlockViewModel : BaseViewModel
{
    public string Id { get; }
    public string DataType { get; }
    public Layout? Layout { get; }

    public ObservableRangeCollection<Button> Buttons { get; }
    
    public Badge Badge { get; }
    
    public ObservableRangeCollection<Text> Texts { get; }
    
    public ObservableRangeCollection<Audio> Audios { get; }
    
    public ObservableRangeCollection<Playlist> Playlists { get; }
    
    public ObservableRangeCollection<CatalogBanner> CatalogBanners { get; }
    
    public ObservableRangeCollection<Link> Links { get; }
    
    public ObservableRangeCollection<Suggestion> Suggestions { get; }
    
    public ObservableRangeCollection<Artist> Artists { get; }
    
    public ObservableRangeCollection<Group> Groups { get; }
    
    public ObservableRangeCollection<RecommendedPlaylist> RecommendedPlaylists { get; }
    
    public ObservableRangeCollection<Video> Videos { get; }
    
    public ObservableRangeCollection<Video> ArtistVideos { get; }
    
    public ObservableRangeCollection<Placeholder> Placeholders { get; }
    
    public ObservableRangeCollection<MusicOwner> MusicOwners { get; }
    
    public ObservableRangeCollection<AudioFollowingsUpdateInfo> FollowingsUpdateInfos { get; }
    
    public ObservableRangeCollection<Station> Stations { get; }
    
    public ObservableRangeCollection<Curator> Curators { get; }
    
    public ObservableRangeCollection<Longread> Longreads { get; }
    
    public ImmutableHashSet<string> ListenEvents { get; private set; }
    
    public BlockViewModel(Block block)
    {
        Id = block.Id;
        DataType = block.DataType;
        Layout = block.Layout;
        Buttons = new(block.Buttons);
        Buttons.AddRange(block.Actions);
        Badge = block.Badge;
        Texts = new(block.Texts);
        Audios = new(block.Audios);
        Playlists = new(block.Playlists);
        CatalogBanners = new(block.Banners);
        Links = new(block.Links);
        Suggestions = new(block.Suggestions);
        Artists = new(block.Artists);
        Groups = new(block.Groups);
        RecommendedPlaylists = new(block.RecommendedPlaylists);
        Videos = new(block.Videos);
        ArtistVideos = new(block.ArtistVideos);
        Placeholders = new(block.Placeholders);
        MusicOwners = new(block.MusicOwners);
        FollowingsUpdateInfos = new(block.FollowingsUpdateInfos);
        Stations = new(block.Stations);
        Curators = new(block.Curators);
        Longreads = new(block.Longreads);
        ListenEvents = [..block.ListenEvents];
    }

    public void MergeBlock(Block block)
    {
        const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;
        Buttons.AddRange(block.Buttons, action);
        Buttons.AddRange(block.Actions, action);
        Texts.AddRange(block.Texts, action);
        Audios.AddRange(block.Audios, action);
        Playlists.AddRange(block.Playlists, action);
        CatalogBanners.AddRange(block.Banners, action);
        Links.AddRange(block.Links, action);
        Suggestions.AddRange(block.Suggestions, action);
        Artists.AddRange(block.Artists, action);
        Groups.AddRange(block.Groups, action);
        RecommendedPlaylists.AddRange(block.RecommendedPlaylists, action);
        Videos.AddRange(block.Videos, action);
        ArtistVideos.AddRange(block.ArtistVideos, action);
        Placeholders.AddRange(block.Placeholders, action);
        MusicOwners.AddRange(block.MusicOwners, action);
        FollowingsUpdateInfos.AddRange(block.FollowingsUpdateInfos, action);
        Stations.AddRange(block.Stations, action);
        Curators.AddRange(block.Curators, action);
        Longreads.AddRange(block.Longreads, action);
        ListenEvents = ListenEvents.Union(block.ListenEvents);
    }
}