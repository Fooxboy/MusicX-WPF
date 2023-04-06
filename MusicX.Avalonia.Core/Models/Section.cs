namespace MusicX.Avalonia.Core.Models;

public record Section(string Id, string Title, string Url, ICollection<SectionBlock> Blocks, string? NextFrom);