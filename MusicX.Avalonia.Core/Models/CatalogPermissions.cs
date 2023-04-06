namespace MusicX.Avalonia.Core.Models;

public record CatalogPermissions(
    bool Play,
    bool Share,
    bool Edit,
    bool Follow,
    bool Delete,
    bool BoomDownload,
    bool SaveAsCopy
);