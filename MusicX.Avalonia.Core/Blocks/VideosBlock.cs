using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record VideosBlock(string Id,
                          string DataType,
                          SectionBlockLayout Layout,
                          string? NextFrom,
                          string? Url,
                          IReadOnlyList<CatalogVideo> Videos) : BlockBase(Id, DataType, Layout, NextFrom, Url);