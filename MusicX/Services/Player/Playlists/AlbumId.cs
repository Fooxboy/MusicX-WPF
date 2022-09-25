using System;

namespace MusicX.Services.Player.Playlists;

public abstract record AlbumId(string Name, string CoverUrl);

public sealed record VkAlbumId
    (long Id, long OwnerId, string AccessKey, string Name, string CoverUrl) : AlbumId(Name, CoverUrl)
{
    public bool Equals(VkAlbumId? other)
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

public sealed record BoomAlbumId(string Id, string Name, string CoverUrl) : AlbumId(Name, CoverUrl)
{
    public bool Equals(BoomAlbumId? other) => Id == other?.Id;
    public override int GetHashCode() => Id.GetHashCode();
}