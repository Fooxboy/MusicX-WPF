using System.Buffers;
using System.Diagnostics;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition.Server;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace MusicX.Avalonia.Rendering;

public class BlurImageRender : AuraDrawOperationBase
{
    public override void Init(Rect src, Rect dest, float levelX, float levelY, float cornerRadius)
    {
        base.Init(src, dest, levelX, levelY, cornerRadius);
        _levelX = levelX;
        _levelY = levelY;
        _dest = dest;
        _cornerRadius = cornerRadius;
    }
    
    private Rect _dest;
    private float _levelX;
    private float _levelY;
    private float _cornerRadius;

    public BlurImageRender(Stream data)
    {
        var bitmap = SKBitmap.Decode(data);
        Image = SKImage.FromBitmap(bitmap);
    }
    
    public BlurImageRender(SKBitmap data)
    {
        Image = SKImage.FromBitmap(data);
    }

    public SKImage Image { get; }

    public override void Render(IDrawingContextImpl drwContext)
    {
        base.Render(drwContext);

        if (drwContext is not CompositorDrawingContextProxy context) return;
        
        var canvas = ((DrawingContextImpl)context._impl).Canvas;
        using var paint = new SKPaint();

        paint.ImageFilter = SKImageFilter.CreateBlur(_levelX, _levelY);
        
        canvas.ClipRoundRect(new(Bounds.ToSKRect(), _cornerRadius));
        canvas.DrawImage(Image, _dest.ToSKRect(), Bounds.ToSKRect(), paint);
    }
}

public abstract class AuraDrawOperationBase : ICustomDrawOperation
{
    public virtual void Init(Rect src, Rect dest, float levelX, float levelY, float cornerRadius)
    {
        Bounds = src;
    }

    public virtual Rect Bounds { get; private set; }

    public virtual void Dispose()
    {
        // do nothing
    }

    public virtual bool Equals(ICustomDrawOperation? other) => false;

    public virtual bool HitTest(Point p) => true;

    public virtual void Render(IDrawingContextImpl context)
    {
    }
}