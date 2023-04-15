using MusicX.Avalonia.Core.Models;
using VkApi;
using VkApi.Core;
using VkApi.Core.Requests;

namespace MusicX.Avalonia.Core.Extensions;

public static class VkApiExtensions
{
    public static Task<CatalogGetAudioResponse> GetCatalogAudioAsync(this Api api, CatalogGetAudioRequest request) =>
        api.Client.RequestAsync("catalog.getAudio",
                                JsonContext.Default.CatalogGetAudioRequest,
                                MusicXJsonContext.Default.CatalogGetAudioResponse, 
                                request);
    
    public static Task<CatalogGetSectionResponse> GetCatalogSectionAsync(this Api api, CatalogGetSectionRequest request) =>
        api.Client.RequestAsync("catalog.getSection",
                                JsonContext.Default.CatalogGetSectionRequest,
                                MusicXJsonContext.Default.CatalogGetSectionResponse, 
                                request);
    
    public static Task<ICollection<UsersGetUser>> GetUsersAsync(this Api api, UsersGetRequest request) =>
        api.Client.RequestAsync("users.get",
                                JsonContext.Default.UsersGetRequest,
                                MusicXJsonContext.Default.ICollectionUsersGetUser, 
                                request);
    
    public static Task<ExecuteGetPlaylistResponse> GetPlaylistAsync(this Api api, ExecuteGetPlaylistRequest request) => 
        api.Client.RequestAsync("execute.getPlaylist",
                                MusicXJsonContext.Default.ExecuteGetPlaylistRequest,
                                MusicXJsonContext.Default.ExecuteGetPlaylistResponse, 
                                request);
    
    public static Task<AudioGetResponse> GetAudioAsync(this Api api, AudioGetRequest request) =>
        api.Client.RequestAsync("audio.get",
                                JsonContext.Default.AudioGetRequest,
                                MusicXJsonContext.Default.AudioGetResponse, 
                                request);
    
    public static Task<CatalogGetAudioResponse> GetCatalogAudioArtistAsync(this Api api, CatalogGetAudioArtistRequest request) =>
        api.Client.RequestAsync("catalog.getAudioArtist",
                                JsonContext.Default.CatalogGetAudioArtistRequest,
                                MusicXJsonContext.Default.CatalogGetAudioResponse, 
                                request);
}