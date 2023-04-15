using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record ActionBlock(string Id,
                          string DataType,
                          SectionBlockLayout Layout,
                          string? NextFrom,
                          string? Url,
                          ICollection<CatalogAction> Actions) : BlockBase(Id, DataType, Layout, NextFrom, Url);
                          
public record LinksBlock(string Id,
                          string DataType,
                          SectionBlockLayout Layout,
                          string? NextFrom,
                          string? Url,
                          ICollection<CatalogLink> Links) : BlockBase(Id, DataType, Layout, NextFrom, Url);