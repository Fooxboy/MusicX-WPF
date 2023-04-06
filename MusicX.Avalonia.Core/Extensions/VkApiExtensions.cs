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
}