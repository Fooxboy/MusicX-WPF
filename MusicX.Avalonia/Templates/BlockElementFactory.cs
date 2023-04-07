using System.Collections.Frozen;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MusicX.Avalonia.Core.Blocks;
using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Templates;

public class BlockElementFactory : ElementFactory
{
    private FrozenDictionary<(string DataType, string LayoutName), BlockTemplate>? _templates = null;

    public RecyclePool Pool { get; } = new();

    [Content] public ICollection<BlockTemplate> Templates { get; } = new List<BlockTemplate>();
    
    public IDataTemplate? FallbackTemplate { get; set; }

    protected override Control GetElementCore(ElementFactoryGetArgs args)
    {
        if (args.Data is not BlockBase block)
            throw new ArgumentException(null, nameof(args));

        if (Pool.TryGetElement(block.Id, args.Parent) is { } element)
            return element;

        _templates ??= Templates.ToFrozenDictionary(b => (b.BlockDataType, b.LayoutName));
        
        if (!_templates.TryGetValue((block.DataType, block.Layout.Name), out var template))
        {
            if (FallbackTemplate is null)
                throw new KeyNotFoundException(
                    $"Missing template for {block.DataType} with layout {block.Layout.Name}");
            
            if (Pool.TryGetElement(block.Id, args.Parent) is { } fallbackElement)
                return fallbackElement;

            fallbackElement = FallbackTemplate.Build(args.Data)!;
            Pool.SetReuseKey(fallbackElement, "FallbackBlock");
            
            return fallbackElement;
        }
        
        element = template.Build(args.Data, args.Parent);
        Pool.SetReuseKey(element, block.Id);
        
        return element;
    }

    protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
    {
        var element = args.Element!;
        var key = Pool.GetReuseKey(element);
        Pool.PutElement(element, key, args.Parent);
    }
}