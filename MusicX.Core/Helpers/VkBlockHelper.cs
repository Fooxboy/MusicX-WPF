using MusicX.Core.Models;
using MusicX.Core.Models.General;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MusicX.Core.Helpers
{
    public static class VkBlockHelper
    {

        public static ResponseVk Proccess(this ResponseVk response)
        {

            if(response.Response.Playlists != null)
            {
                foreach(var playlist in response.Response.Playlists)
                {
                    if (playlist.Original != null)
                    {
                        if (playlist.Original?.OwnerId < 0)
                        {
                            var id = (playlist.Original.OwnerId * -1);
                            if(response.Response.Groups != null)
                            {
                                var value = response.Response.Groups.SingleOrDefault(g => g.Id == id);
                                playlist.OwnerName = value?.Name;

                            }

                        }
                        else
                        {
                            var value = response.Response.Profiles.SingleOrDefault(p => p.Id == playlist.Original?.OwnerId);

                            playlist.OwnerName = value?.FirstName + " " + value?.LastName;
                        }
                    }
                    else
                    {
                        if (playlist.OwnerId < 0)
                        {
                            var id = (playlist.OwnerId * -1);
                            if(response.Response.Groups != null)
                            {
                                var value = response.Response.Groups.SingleOrDefault(g => g.Id == id);

                                playlist.OwnerName = value?.Name;
                            }
                        }
                        else
                        {
                            var value = response.Response.Profiles.SingleOrDefault(p => p.Id == playlist?.OwnerId);

                            playlist.OwnerName = value?.FirstName + " " + value?.LastName;
                        }
                    }
                }
            }

            if(response.Response.Replacements != null)
            {
                foreach(var replaceModel in response.Response.Replacements.ReplacementsModels)
                {
                    foreach(var block in replaceModel.ToBlocks)
                    {
                        if (block.AudiosIds != null || block.AudiosIds.Count > 0)
                        {
                            foreach (var audioStringId in block.AudiosIds)
                            {
                                var audioArray = audioStringId.Split('_');

                                var audioId = long.Parse(audioArray[1]);
                                var ownerId = long.Parse(audioArray[0]);

                                var audio = response.Response.Audios.SingleOrDefault(a => a.Id == audioId && a.OwnerId == ownerId);

                                if (audio == null) continue;

                                audio.ParentBlockId = block.Id;
                                block.Audios.Add(audio);

                            }
                        }

                        if (block.PlaylistsIds != null || block.PlaylistsIds.Count > 0)
                        {
                            foreach (var playlistStringId in block.PlaylistsIds)
                            {
                                var playlistArray = playlistStringId.Split('_');

                                var playlistId = long.Parse(playlistArray[1]);
                                var ownerId = long.Parse(playlistArray[0]);

                                var playlist = response.Response.Playlists.SingleOrDefault(p => p.Id == playlistId && p.OwnerId == ownerId);

                                if (playlist == null) continue;

                                block.Playlists.Add(playlist);

                            }
                        }

                        if (block.CatalogBannerIds != null || block.CatalogBannerIds.Count > 0)
                        {
                            foreach (var bannerId in block.CatalogBannerIds)
                            {

                                var banner = response.Response.CatalogBanners.SingleOrDefault(b => b.Id == bannerId);

                                if (banner == null) continue;

                                block.Banners.Add(banner);

                            }
                        }

                        if (block.LinksIds != null || block.LinksIds.Count > 0)
                        {
                            foreach (var linkId in block.LinksIds)
                            {

                                var link = response.Response.Links.SingleOrDefault(b => b.Id == linkId);

                                if (link == null) continue;

                                block.Links.Add(link);

                            }
                        }

                        if (block.SuggestionsIds != null || block.SuggestionsIds.Count > 0)
                        {
                            foreach (var suggestionId in block.SuggestionsIds)
                            {
                                var suggestion = response.Response.Suggestions.SingleOrDefault(b => b.Id == suggestionId);

                                if (suggestion == null) continue;

                                block.Suggestions.Add(suggestion);

                            }
                        }

                        if (block.ArtistsIds != null || block.ArtistsIds.Count > 0)
                        {
                            foreach (var artistId in block.ArtistsIds)
                            {
                                var artist = response.Response.Artists.SingleOrDefault(b => b.Id == artistId);

                                if (artist == null) continue;

                                block.Artists.Add(artist);

                            }
                        }

                        if (block.TextIds != null || block.TextIds.Count > 0)
                        {
                            foreach (var textId in block.TextIds)
                            {
                                var text = response.Response.Texts.SingleOrDefault(b => b.Id == textId);

                                if (text == null) continue;

                                block.Texts.Add(text);

                            }
                        }

                        if (block.GroupIds != null || block.GroupIds.Count > 0)
                        {
                            foreach (var groupId in block.GroupIds)
                            {
                                var group = response.Response.Groups.SingleOrDefault(b => b.Id == groupId);

                                if (group == null) continue;

                                block.Groups.Add(group);

                            }
                        }

                        if (block.CuratorsIds != null || block.CuratorsIds.Count > 0)
                        {
                            foreach (var curatorId in block.CuratorsIds)
                            {
                                var curator = response.Response.Curators.SingleOrDefault(b => b.Id == curatorId);

                                if (curator == null) continue;

                                block.Curators.Add(curator);

                            }
                        }
                    }
                }
            }

            if (response.Response.Block != null)
            {
                
                if (response.Response.Block.AudiosIds != null || response.Response.Block.AudiosIds.Count > 0)
                {
                    foreach (var audioStringId in response.Response.Block.AudiosIds)
                    {
                        var audioArray = audioStringId.Split('_');

                        var audioId = long.Parse(audioArray[1]);
                        var ownerId = long.Parse(audioArray[0]);

                        var audio = response.Response.Audios.SingleOrDefault(a => a.Id == audioId && a.OwnerId == ownerId);

                        if (audio == null) continue;

                        audio.ParentBlockId = response.Response.Block.Id;

                        response.Response.Block.Audios.Add(audio);

                    }
                }

                if (response.Response.Block.PlaylistsIds != null || response.Response.Block.PlaylistsIds.Count > 0)
                {
                    foreach (var playlistStringId in response.Response.Block.PlaylistsIds)
                    {
                        var playlistArray = playlistStringId.Split('_');

                        var playlistId = long.Parse(playlistArray[1]);
                        var ownerId = long.Parse(playlistArray[0]);

                        var playlist = response.Response.Playlists.SingleOrDefault(p => p.Id == playlistId && p.OwnerId == ownerId);

                        if (playlist == null) continue;

                        response.Response.Block.Playlists.Add(playlist);

                    }
                }

                if (response.Response.Block.CatalogBannerIds != null || response.Response.Block.CatalogBannerIds.Count > 0)
                {
                    foreach (var bannerId in response.Response.Block.CatalogBannerIds)
                    {


                        var banner = response.Response.CatalogBanners.SingleOrDefault(b => b.Id == bannerId);

                        if (banner == null) continue;

                        response.Response.Block.Banners.Add(banner);

                    }
                }

                if (response.Response.Block.LinksIds != null || response.Response.Block.LinksIds.Count > 0)
                {
                    foreach (var linkId in response.Response.Block.LinksIds)
                    {
                        var link = response.Response.Links.SingleOrDefault(b => b.Id == linkId);

                        if (link == null) continue;

                        response.Response.Block.Links.Add(link);

                    }
                }

                if(response.Response.Block.SuggestionsIds != null || response.Response.Block.SuggestionsIds.Count > 0)
                {
                    foreach (var suggestionId in response.Response.Block.SuggestionsIds)
                    {
                        var suggestion = response.Response.Suggestions.SingleOrDefault(b => b.Id == suggestionId);

                        if (suggestion == null) continue;

                        response.Response.Block.Suggestions.Add(suggestion);

                    }
                }

                if (response.Response.Block.ArtistsIds != null || response.Response.Block.ArtistsIds.Count > 0)
                {
                    foreach (var artistId in response.Response.Block.ArtistsIds)
                    {
                        var artist = response.Response.Artists.SingleOrDefault(b => b.Id == artistId);

                        if (artist == null) continue;

                        response.Response.Block.Artists.Add(artist);

                    }
                }

                if (response.Response.Block.TextIds != null || response.Response.Block.TextIds.Count > 0)
                {
                    foreach (var textId in response.Response.Block.TextIds)
                    {
                        var text = response.Response.Texts.SingleOrDefault(b => b.Id == textId);

                        if (text == null) continue;

                        response.Response.Block.Texts.Add(text);

                    }
                }

                if (response.Response.Block.GroupIds != null || response.Response.Block.GroupIds.Count > 0)
                {
                    foreach (var groupId in response.Response.Block.GroupIds)
                    {
                        var group = response.Response.Groups.SingleOrDefault(b => b.Id == groupId);

                        if (group == null) continue;

                        response.Response.Block.Groups.Add(group);

                    }
                }

                if (response.Response.Block.CuratorsIds != null || response.Response.Block.CuratorsIds.Count > 0)
                {
                    foreach (var curatorId in response.Response.Block.CuratorsIds)
                    {
                        var curator = response.Response.Curators.SingleOrDefault(b => b.Id == curatorId);

                        if (curator == null) continue;

                        response.Response.Block.Curators.Add(curator);

                    }
                }

            }

            if (response.Response.Section == null) return response;

            foreach (var block in response.Response.Section.Blocks)
            {
                if(block.AudiosIds != null || block.AudiosIds.Count > 0)
                {
                    foreach(var audioStringId in block.AudiosIds)
                    {
                        var audioArray = audioStringId.Split('_');

                        var audioId = long.Parse(audioArray[1]);
                        var ownerId = long.Parse(audioArray[0]);

                        var audio = response.Response.Audios.SingleOrDefault(a=> a.Id == audioId && a.OwnerId == ownerId);

                        if (audio == null) continue;

                        audio.ParentBlockId = block.Id;

                        block.Audios.Add(audio);
                        
                    }
                }

                if (block.PlaylistsIds != null || block.PlaylistsIds.Count > 0)
                {

                    if(block.DataType == "music_recommended_playlists")
                    {
                        foreach (var lid in block.PlaylistsIds)
                        {
                            var p = response.Response.RecommendedPlaylists.SingleOrDefault(b => b.OwnerId + "_" + b.Id == lid);

                            if (p == null) continue;

                            var pp = response.Response.Playlists.SingleOrDefault(b => b.OwnerId + "_" + b.Id == lid);

                            p.Playlist = pp;
                            block.RecommendedPlaylists.Add(p);

                        }

                        foreach (var r in block.RecommendedPlaylists)
                        {
                            foreach (var aid in r.AudiosIds)
                            {
                                var a = response.Response.Audios.SingleOrDefault(b => b.OwnerId + "_" + b.Id == aid);

                                if (a == null) continue;

                                r.Audios.Add(a);
                            }
                        }



                    }
                    else
                    {
                        foreach (var playlistStringId in block.PlaylistsIds)
                        {
                            var playlistArray = playlistStringId.Split('_');

                            var playlistId = long.Parse(playlistArray[1]);
                            var ownerId = long.Parse(playlistArray[0]);

                            var playlist = response.Response.Playlists.SingleOrDefault(p => p.Id == playlistId && p.OwnerId == ownerId);

                            if (playlist == null) continue;

                            block.Playlists.Add(playlist);

                        }
                    }
                }

                if (block.CatalogBannerIds != null || block.CatalogBannerIds.Count > 0)
                {
                    foreach (var bannerId in block.CatalogBannerIds)
                    {
                       
                        var banner = response.Response.CatalogBanners.SingleOrDefault(b => b.Id == bannerId );

                        if (banner == null) continue;

                        block.Banners.Add(banner);

                    }
                }

                if (block.LinksIds != null || block.LinksIds.Count > 0)
                {
                    foreach (var linkId in block.LinksIds)
                    {

                        var link = response.Response.Links.SingleOrDefault(b => b.Id == linkId);

                        if (link == null) continue;

                        block.Links.Add(link);

                    }
                }

                if (block.SuggestionsIds != null || block.SuggestionsIds.Count > 0)
                {
                    foreach (var suggestionId in block.SuggestionsIds)
                    {
                        var suggestion = response.Response.Suggestions.SingleOrDefault(b => b.Id == suggestionId);

                        if (suggestion == null) continue;

                        block.Suggestions.Add(suggestion);

                    }
                }

                if (block.ArtistsIds != null || block.ArtistsIds.Count > 0)
                {
                    foreach (var artistId in block.ArtistsIds)
                    {
                        var artist = response.Response.Artists.SingleOrDefault(b => b.Id == artistId);

                        if (artist == null) continue;

                        block.Artists.Add(artist);

                    }
                }

                if (block.TextIds != null || block.TextIds.Count > 0)
                {
                    foreach (var textId in block.TextIds)
                    {
                        var text = response.Response.Texts.SingleOrDefault(b => b.Id == textId);

                        if (text == null) continue;

                        block.Texts.Add(text);

                    }
                }

                if (block.GroupIds != null || block.GroupIds.Count > 0)
                {
                    foreach (var groupId in block.GroupIds)
                    {
                        var group = response.Response.Groups.SingleOrDefault(b => b.Id == groupId);

                        if (group == null) continue;

                        block.Groups.Add(group);

                    }
                }

                if (block.CuratorsIds != null || block.CuratorsIds.Count > 0)
                {
                    foreach (var curatorId in block.CuratorsIds)
                    {
                        var curator = response.Response.Curators.SingleOrDefault(b => b.Id == curatorId);

                        if (curator == null) continue;

                        block.Curators.Add(curator);

                    }
                }

                if (block.PodcastSliderItemsIds.Count > 0)
                {
                    foreach (var psid in block.PodcastSliderItemsIds)
                    {
                        try
                        {
                            var ps = response.Response.PodcastSliderItems.SingleOrDefault(b => b.ItemId == psid);
                            if (ps == null) continue;

                            block.PodcastSliderItems.Add(ps);

                        }catch(Exception ex)
                        {
                            Debug.WriteLine($"Error in psid: {psid}");
                        }
                    }
                }

                if (block.PodcastEpisodesIds.Count > 0)
                {
                    foreach (var peid in block.PodcastEpisodesIds)
                    {
                        var pe = response.Response.PodcastEpisodes.SingleOrDefault(b => b.OwnerId+"_"+b.Id == peid);

                        if (pe == null) continue;

                        block.PodcastEpisodes.Add(pe);

                    }
                }

                if (block.LongreadsIds.Count > 0)
                {
                    foreach (var lid in block.LongreadsIds)
                    {
                        var l = response.Response.Longreads.SingleOrDefault(b => b.OwnerId + "_" + b.Id == lid);

                        if (l == null) continue;

                        block.Longreads.Add(l);

                    }
                }

                if (block.VideosIds.Count > 0)
                {
                    foreach (var vid in block.VideosIds)
                    {
                        var v = response.Response.Videos.SingleOrDefault(b => b.OwnerId + "_" + b.Id == vid);

                        if (v == null) continue;

                        block.Videos.Add(v);

                    }
                }

                if (block.ArtistVideosIds.Count > 0)
                {
                    foreach (var vid in block.ArtistVideosIds)
                    {
                        var v = response.Response.ArtistVideos.SingleOrDefault(b => b.OwnerId + "_" + b.Id == vid);

                        if (v == null) continue;

                        block.ArtistVideos.Add(v);

                    }
                }

            }

          
            return response;
        }
    }
}
