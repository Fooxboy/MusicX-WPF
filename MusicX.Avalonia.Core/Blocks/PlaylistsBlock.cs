using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record PlaylistsBlock(string Id,
                             string DataType,
                             SectionBlockLayout Layout,
                             string? NextFrom,
                             string? Url,
                             ICollection<CatalogPlaylist> Playlists) : BlockBase(Id, DataType, Layout, NextFrom, Url);