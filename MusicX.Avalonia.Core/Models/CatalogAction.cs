namespace MusicX.Avalonia.Core.Models;

public record CatalogAction(
    CatalogAction Action,
    string Title,
    int RefItemsCount,
    string RefLayoutName,
    string RefDataType,
    string SectionId,
    string BlockId,
    string? Url
);