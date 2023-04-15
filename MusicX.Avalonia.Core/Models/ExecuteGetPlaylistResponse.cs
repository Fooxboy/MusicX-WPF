namespace MusicX.Avalonia.Core.Models;

public record ExecuteGetPlaylistResponse(ICollection<CatalogAudio> Audios, ICollection<CatalogMusicOwner>? MusicOwners);