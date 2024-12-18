using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.ViewModels;
using MusicX.Views;

namespace MusicX.Helpers;

public static partial class NavigationServiceExtensions
{
    public static async Task OpenLinkAsync(this NavigationService navigationService, Link link)
    {
        var vkService = StaticService.Container.GetRequiredService<VkService>();
        
        switch (link.Meta.ContentType)
        {
            case "" or null or "audio_recent" or "audio_followings":
            {
                var match = PodcastsCategoryUrl().Match(link.Url);

                if (match.Success)
                {
                    //var podcasts = await vkService.GetPodcastsAsync(Link.Url);
                    //await navigationService.OpenSection(podcasts.Catalog.DefaultSection, true);

                    return;
                }
                
                match = MiniAppRegex().Match(link.Url);
                
                if (match.Success)
                {
                    var appId = match.Groups["app"].Value;
                    var miniAppResponse = await vkService.GetMiniApp(appId, link.Url);
                    
                    navigationService.OpenExternalPage(new MiniAppView(new MiniAppViewModel(appId, miniAppResponse.Object.WebviewUrl)));
                    return;
                }

                var music = await vkService.GetAudioCatalogAsync(link.Url);
                navigationService.OpenSection(music.Catalog.DefaultSection);

                return;
            }
            case "artist":
            {
                var url = new Uri(link.Url);

                navigationService.OpenSection(url.Segments.Last(), SectionType.Artist);
                break;
            }
            case "group" or "user" or "chat" when CustomSectionsService.CustomLinkRegex().IsMatch(link.Id):
            {
                navigationService.OpenSection(link.Id);
                return;
            }
            case "group" or "user" or "chat":
            {
                var match = UserProfileRegex().Match(link.Url);
                if (match.Success)
                {
                    var music = await vkService.GetAudioCatalogAsync(link.Url);

                    navigationService.OpenSection(music.Catalog.DefaultSection);

                    return;
                }


                Process.Start(new ProcessStartInfo
                {
                    FileName = link.Url,
                    UseShellExecute = true
                });
                break;
            }
            case "curator":
            {
                var curator = await vkService.GetAudioCuratorAsync(link.Meta.TrackCode, link.Url);

                navigationService.OpenSection(curator.Catalog.DefaultSection);
                break;
            }
            case "audio_playlists" or "audio_albums":
            {
                var match = PlaylistOrAlbumUrl().Match(link.Url);
                if (match.Success)
                {
                    var (playlistId, ownerId, accessKey, _) = PlaylistData.Parse(match.Groups["id"].Value);
                    navigationService.OpenExternalPage(new PlaylistView(playlistId, ownerId, accessKey));
                    return;
                }

                if (link.Url == "https://vk.com/audio?catalog=my_audios")
                {
                    navigationService.OpenSection("my_audios");
                    return;
                }

                var catalog = await vkService.GetAudioCatalogAsync(link.Url);

                navigationService.OpenSection(catalog.Catalog.DefaultSection);
                break;
            }
            case "custom":
            {
                navigationService.OpenSection(link.Meta.TrackCode);
                break;
            }
        }
    }

    [GeneratedRegex(@"https://vk\.com/podcasts\?category=[0-9]+$")]
    private static partial Regex PodcastsCategoryUrl();
    
    [GeneratedRegex(@"https://vk\.com/audios\-?[0-9]+$")]
    private static partial Regex UserProfileRegex();

    [GeneratedRegex(@"https://vk\.com/music/(?:album|playlist)/(?<id>[\-\w]+)$")]
    private static partial Regex PlaylistOrAlbumUrl();
    
    [GeneratedRegex(@"https://vk\.com/(?<app>app\-?[0-9]+)(?>[\?\#].+)?$")]
    private static partial Regex MiniAppRegex();
}