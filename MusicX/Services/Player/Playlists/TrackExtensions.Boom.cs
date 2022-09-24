using System;
using System.Linq;
using MusicX.Core.Models.Boom;

namespace MusicX.Services.Player.Playlists;

public static partial class TrackExtensions
{
    public static PlaylistTrack ToTrack(this Track track)
    {
        var mainArtists = track.Artists?.Any() == false
            ? new[] { track.Artist.ToTrackArtist() }
            : track.Artists!.Select(ToTrackArtist).ToArray();

        return new(track.Name, string.Empty, track.Album?.ToAlbumId(), mainArtists, Array.Empty<TrackArtist>(),
            new(track.File, track.IsLiked, track.IsExplicit, TimeSpan.FromSeconds(track.Duration)));
    }

    public static TrackArtist ToTrackArtist(this Artist artist)
    {
        return new(artist.Name, new(artist.ApiId, ArtistIdType.Boom));
    }

    public static BoomAlbumId ToAlbumId(this Album album)
    {
        return new(album.ApiId, album.Name, album.Cover.Url);
    }
}