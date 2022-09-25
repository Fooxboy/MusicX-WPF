using System;
using System.Collections.Generic;
using MusicX.ViewModels;

namespace MusicX.Services.Player.Playlists;

public sealed record PlaylistTrack(string Title, string Subtitle, AlbumId? AlbumId,
                                   ICollection<TrackArtist> MainArtists,
                                   ICollection<TrackArtist> FeaturedArtists, TrackData Data)
{
    public bool Equals(PlaylistTrack? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return AlbumId == other.AlbumId && Data == other.Data;
    }
    public override int GetHashCode() => HashCode.Combine(AlbumId?.GetHashCode(), Data.GetHashCode());
}

public abstract record TrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration);

public sealed record BoomTrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration, string Id) : TrackData(
    Url, IsLiked, IsExplicit, Duration)
{
    public bool Equals(BoomTrackData? other) => Id == other?.Id;
    public override int GetHashCode() => Id.GetHashCode();
}

public sealed record VkTrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration,
                          long Id, long OwnerId, string AccessKey) : TrackData(Url, IsLiked, IsExplicit, Duration)
{
    public bool Equals(VkTrackData? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        
        return Id == other.Id && OwnerId == other.OwnerId && AccessKey == other.AccessKey;
    }
    public override int GetHashCode() => 
        HashCode.Combine(Id.GetHashCode(), OwnerId.GetHashCode(), AccessKey.GetHashCode());
}
                          
public record DownloaderData
    (string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration, string PlaylistName) : TrackData(
        Url, IsLiked, IsExplicit, Duration);