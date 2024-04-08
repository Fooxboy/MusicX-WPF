using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MusicX.Helpers;

[ContentProperty(nameof(PrimaryTemplate))]
public class NullTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NullTemplate { get; set; }
    
    public DataTemplate? PrimaryTemplate { get; set; }
    
    public override DataTemplate SelectTemplate(object? item, DependencyObject container)
    {
        return (item is null ? NullTemplate : PrimaryTemplate) ?? throw new("NullTemplate and PrimaryTemplate must be set");
    }
}