using Avalonia.Markup.Xaml.Templates;

namespace MusicX.Avalonia.Templates;

public class KeyedTemplate : DataTemplate
{
    public required string DataKey { get; set; }
}