using MusicX.Controls;
using MusicX.Core.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using MusicX.ViewModels;

namespace MusicX.Helpers;

[ContentProperty(nameof(FallbackTemplate))]
public class BlockTemplateSelector : DataTemplateSelector
{
    public DataTemplate? FallbackTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (container is not ContentPresenter { TemplatedParent: BlockControl control })
            throw new ArgumentException();

        return (item is BlockViewModel block
            ? control.TryFindResource(string.IsNullOrEmpty(block.Layout?.Name) ? block.DataType : $"{block.DataType}_{block.Layout.Name}") ?? control.TryFindResource(block.DataType) ?? FallbackTemplate
            : FallbackTemplate) as DataTemplate;
    }
}
