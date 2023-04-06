namespace MusicX.Avalonia.Core.Models;

public record Catalog(string DefaultSection, ICollection<CatalogSection> Sections);