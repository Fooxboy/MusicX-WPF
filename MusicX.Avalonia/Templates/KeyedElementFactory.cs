using System.Collections.Frozen;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;

namespace MusicX.Avalonia.Templates;

public class KeyedElementFactory : ElementFactory
{
    private FrozenDictionary<string, KeyedTemplate>? _templates;

    public RecyclePool Pool { get; } = new();

    [Content] public ICollection<KeyedTemplate> Templates { get; } = new List<KeyedTemplate>();
    
    public IDataTemplate? FallbackTemplate { get; set; }

    protected override Control GetElementCore(ElementFactoryGetArgs args)
    {
        if (args.Data is not { } data)
            throw new ArgumentException(null, nameof(args));
        
        var key = data.ToString()!;

        if (Pool.TryGetElement(key, args.Parent) is { } element)
            return element;

        _templates ??= Templates.ToFrozenDictionary(b => b.DataKey);
        
        if (!_templates.TryGetValue(key, out var template))
        {
            if (FallbackTemplate is null)
                throw new KeyNotFoundException(
                    $"Missing template for {key}");

            var fallbackElement = FallbackTemplate.Build(args.Data)!;
            Pool.SetReuseKey(fallbackElement, "FallbackBlock");
            
            return fallbackElement;
        }
        
        element = template.Build(args.Data, args.Parent);
        Pool.SetReuseKey(element, key);
        
        return element;
    }

    protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
    {
        var element = args.Element!;
        var key = Pool.GetReuseKey(element);
        Pool.PutElement(element, key, args.Parent);
    }
}