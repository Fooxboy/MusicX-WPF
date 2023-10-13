using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Shared.Player;
using VkNet.Abstractions;

namespace MusicX.Services.Player.Playlists;

public static partial class TrackExtensions
{
    public static PlaylistTrack ToTrack(this Audio audio) => ToTrack(audio, null);
    
    public static PlaylistTrack ToTrack(this Audio audio, Playlist? playlist)
    {
        TrackArtist[] mainArtists;
        if (audio.MainArtists is null || !audio.MainArtists.Any())
            mainArtists = new[] { new TrackArtist(audio.Artist, null) };
        else
            mainArtists = audio.MainArtists.Select(ToTrackArtist).ToArray();

        var isLiked = audio.OwnerId == StaticService.Container.GetRequiredService<IVkApi>().UserId!.Value;

        TrackData trackData;

        if(audio.Url.EndsWith(".mp3"))
        {
            trackData = new BoomTrackData(audio.Url, false, true, TimeSpan.FromSeconds(audio.Duration), audio.Id.ToString());

        }else
        {
            trackData =
                   new VkTrackData(audio.Url, isLiked, audio.IsExplicit, audio.HasLyrics, TimeSpan.FromSeconds(audio.Duration), new(
                                       audio.Id,
                                       audio.OwnerId, audio.AccessKey), audio.TrackCode, audio.ParentBlockId,
                                   playlist is null ? null : new(playlist.Id, playlist.OwnerId, playlist.AccessKey));
        }

        return new(audio.Title, audio.Subtitle, audio.Album?.ToAlbumId(), mainArtists,
                   audio.FeaturedArtists?.Select(ToTrackArtist).ToArray() ?? Array.Empty<TrackArtist>(), trackData);
    }

    public static TrackArtist ToTrackArtist(this MainArtist artist)
    {
        return new(artist.Name, new(artist.Id, ArtistIdType.Vk));
    }

    public static VkAlbumId ToAlbumId(this Album album)
    {
        return new(album.Id, album.OwnerId, album.AccessKey, album.Title, album.Cover, album.Thumb?.Photo1200 ?? album.Thumb?.Photo600);
    }
}