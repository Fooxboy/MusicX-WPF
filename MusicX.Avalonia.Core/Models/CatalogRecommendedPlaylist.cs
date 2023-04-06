namespace MusicX.Avalonia.Core.Models;

public record CatalogRecommendedPlaylist(
    int Id,
    int OwnerId,
    double Percentage,
    string PercentageTitle,
    IReadOnlyList<string> Audios,
    string Color
);