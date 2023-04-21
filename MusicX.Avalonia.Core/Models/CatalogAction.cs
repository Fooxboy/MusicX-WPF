namespace MusicX.Avalonia.Core.Models;

public record CatalogAction(
    CatalogActionMeta Action,
    string Title,
    int RefItemsCount,
    string RefLayoutName,
    string RefDataType,
    string SectionId,
    string BlockId,
    string? Url,
    IReadOnlyList<CatalogActionOption>? Options)
{
    public override string ToString() => Action.Type;
}

public record CatalogActionOption(string ReplacementId, string Text, string Icon);