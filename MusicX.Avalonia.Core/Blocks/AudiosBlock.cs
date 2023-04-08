using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record AudiosBlock(string Id,
                          string DataType,
                          SectionBlockLayout Layout,
                          string? NextFrom,
                          string? Url,
                          ICollection<CatalogAudio> Audios) : BlockBase(Id, DataType, Layout, NextFrom, Url);