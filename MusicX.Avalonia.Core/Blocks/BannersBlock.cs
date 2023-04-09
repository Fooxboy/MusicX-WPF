using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record BannersBlock(string Id,
                           string DataType,
                           SectionBlockLayout Layout,
                           string? NextFrom,
                           string? Url,
                           ICollection<CatalogBanner> Banners) : BlockBase(Id, DataType, Layout, NextFrom, Url);