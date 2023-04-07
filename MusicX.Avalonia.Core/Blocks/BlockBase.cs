using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record BlockBase(string Id,
                        string DataType,
                        SectionBlockLayout Layout,
                        string? NextFrom,
                        string? Url);