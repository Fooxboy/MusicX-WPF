using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record ArtistBlock(string Id,
                          string DataType,
                          SectionBlockLayout Layout,
                          string? NextFrom,
                          string? Url,
                          IReadOnlyList<CatalogMainArtist> Artists,
                          ICollection<CatalogAction> Actions) : BlockBase(Id, DataType, Layout, NextFrom, Url);