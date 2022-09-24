namespace MusicX.Services.Player.Playlists;

public abstract record AlbumId(string Name, string CoverUrl);
public record VkAlbumId(long Id, long OwnerId, string AccessKey, string Name, string CoverUrl) : AlbumId(Name, CoverUrl);
public record BoomAlbumId(string Id, string Name, string CoverUrl) : AlbumId(Name, CoverUrl);