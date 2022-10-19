using System.Text.Json.Serialization;

namespace MusicX.Shared.Player;

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

[JsonDerivedType(typeof(VkTrackData), "vk")]
[JsonDerivedType(typeof(BoomTrackData), "boom")]
public abstract record TrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration);

public sealed record BoomTrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration, string Id) : TrackData(
    Url, IsLiked, IsExplicit, Duration)
{
    public bool Equals(BoomTrackData? other) => Id == other?.Id;
    public override int GetHashCode() => Id.GetHashCode();
}

public record IdInfo(long Id, long OwnerId, string AccessKey)
{
    public string ToOwnerIdString() => $"{OwnerId}_{Id}";
}

public sealed record VkTrackData(string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration,
                                 IdInfo Info, string TrackCode, string? ParentBlockId,
                                 IdInfo? Playlist) : TrackData(Url, IsLiked, IsExplicit, Duration)
{
    public bool Equals(VkTrackData? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        
        return Info == other.Info;
    }
    public override int GetHashCode() => Info.GetHashCode();
}
                          
public record DownloaderData
    (string Url, bool IsLiked, bool IsExplicit, TimeSpan Duration, string PlaylistName) : TrackData(
        Url, IsLiked, IsExplicit, Duration);