using Avalonia.Markup.Xaml.Templates;
using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Templates;

public class BlockTemplate : DataTemplate
{
    public required string BlockDataType { get; set; }
    public required string LayoutName { get; set; }

    public BlockTemplate()
    {
        DataType = typeof(SectionBlock);
    }
}