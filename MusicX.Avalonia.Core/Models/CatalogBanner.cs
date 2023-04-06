namespace MusicX.Avalonia.Core.Models;

public record CatalogBanner(
    int Id,
    CatalogClickAction ClickAction,
    IReadOnlyList<CatalogButton> Buttons,
    IReadOnlyList<CatalogImage> Images,
    string Text,
    string Title,
    string TrackCode,
    string ImageMode
);