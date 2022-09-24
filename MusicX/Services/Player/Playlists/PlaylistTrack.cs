using System;
using System.Collections.Generic;
using MusicX.ViewModels;

namespace MusicX.Services.Player.Playlists;

public record PlaylistTrack(string Title, string Subtitle, AlbumId? AlbumId, ICollection<TrackArtist> MainArtists,
                            ICollection<TrackArtist> FeaturedArtists, TrackData Data);

public record TrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration);

public record VkTrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration,
                          long Id, long OwnerId, string AccessKey) : TrackData(Url, IsLiked, IsExplicit, Duration);
                          
public record DownloaderData
    (string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration, string PlaylistName) : TrackData(
        Url, IsLiked, IsExplicit, Duration);