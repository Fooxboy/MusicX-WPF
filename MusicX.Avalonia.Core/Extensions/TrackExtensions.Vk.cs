using MusicX.Avalonia.Core.Models;
using MusicX.Shared.Player;

namespace MusicX.Avalonia.Core.Extensions;

public static partial class TrackExtensions
{
    public static PlaylistTrack ToTrack(this CatalogAudio audio) => ToTrack(audio, null);

    public static PlaylistTrack ToTrack(this CatalogAudio audio, CatalogPlaylist? playlist)
    {
        TrackArtist[] mainArtists;
        if (audio.MainArtists?.Count is null or 0)
            mainArtists = new[] { new TrackArtist(audio.Artist, null) };
        else
            mainArtists = audio.MainArtists.Select(ToTrackArtist).ToArray();

        var isLiked = false;

        TrackData trackData;

        if (audio.Url.EndsWith(".mp3"))
        {
            trackData = new BoomTrackData(audio.Url, false, true, TimeSpan.FromSeconds(audio.Duration),
                                          audio.Id.ToString());
        }
        else
        {
            trackData =
                new VkTrackData(audio.Url, isLiked, audio.IsExplicit, audio.HasLyrics,
                                TimeSpan.FromSeconds(audio.Duration),
                                new(audio.Id, audio.OwnerId, audio.AccessKey), 
                                audio.TrackCode, audio.ParentBlockId,
                                playlist is null ? null : new(playlist.Id, playlist.OwnerId, playlist.AccessKey));
        }

        return new(audio.Title, audio.Subtitle, audio.Album?.ToAlbumId(), mainArtists,
                   audio.FeaturedArtists?.Select(ToTrackArtist).ToArray() ?? Array.Empty<TrackArtist>(), trackData);
    }

    public static TrackArtist ToTrackArtist(this CatalogMainArtist artist)
    {
        return new(artist.Name, new(artist.Id, ArtistIdType.Vk));
    }

    public static VkAlbumId ToAlbumId(this CatalogAlbum album)
    {
        return new(album.Id, album.OwnerId, album.AccessKey, album.Title, album.Thumb.Photo300);
    }
}