using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record BannersBlock(string Id,
                           string DataType,
                           SectionBlockLayout Layout,
                           string? NextFrom,
                           string? Url,
                           IReadOnlyList<CatalogBanner> Banners) : BlockBase(Id, DataType, Layout, NextFrom, Url);