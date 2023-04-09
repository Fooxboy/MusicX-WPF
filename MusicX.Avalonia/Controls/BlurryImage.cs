using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MusicX.Avalonia.Rendering;
using ReactiveUI;
using SkiaSharp;

namespace MusicX.Avalonia.Controls;

public class BlurryImage : Control
{
    private Rect _srcRect;
    private Rect _dstRect;

    static BlurryImage()
    {
        AffectsRender<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty,
                                   CornerRadiusProperty, SkImageProperty);
        AffectsMeasure<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty,
                                    CornerRadiusProperty, SkImageProperty);
        AffectsArrange<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty,
                                    CornerRadiusProperty, SkImageProperty);

        ClipToBoundsProperty.OverrideDefaultValue<BlurryImage>(true);
    }

    public BlurryImage()
    {
        BoundsProperty.Changed.Subscribe(BoundsChanged);
        SourceProperty.Changed.Subscribe(SourceChanged);
    }

    private void SourceChanged(object? obj)
    {
        var source = Source;
        RxApp.TaskpoolScheduler.Schedule(() =>
        {
            using var stream = new MemoryStream();
            source?.Save(stream);
            if (source is null || stream.Length <= 0) return;

            _render = new BlurImageRender(stream);
            RxApp.MainThreadScheduler.Schedule(InvalidateVisual);
        });
    }

    public void Load(SKBitmap image)
    {
        _render = new BlurImageRender(image);
        RxApp.MainThreadScheduler.Schedule(() => { SkImage = _render.Image; });
    }

    public void Reset()
    {
        Source = null;
        _render = null;
    }

    private void BoundsChanged(object? obj)
    {
        if (_render is null)
            return;
        var viewPort = new Rect(Bounds.Size);
        var sourceSize = new Size(_render.Image.Width, _render.Image.Height);

        var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
        var scaledSize = sourceSize * scale;
        _dstRect = viewPort
                   .CenterRect(new Rect(scaledSize))
                   .Intersect(viewPort);
        _srcRect = new Rect(sourceSize)
            .CenterRect(new Rect(_dstRect.Size / scale));
    }

    public override void Render(DrawingContext context)
    {
        if (_render is null) return;

        _render.Init(_dstRect, _srcRect, BlurLevel, BlurLevel, CornerRadius);
        context.Custom(_render);
    }

    ///<inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var size = Source?.Size ?? (SkImage is null ? null : new Size(SkImage.Width, SkImage.Height));
        var result = new Size();

        if (size.HasValue)
        {
            result = Stretch.CalculateSize(availableSize, size.Value, StretchDirection);
        }

        return result;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = Source?.Size ?? (SkImage is null ? null : new Size(SkImage.Width, SkImage.Height));

        if (size == null) return new Size();
        var sourceSize = size.Value;
        var result = Stretch.CalculateSize(finalSize, sourceSize);
        return result;
    }

    public float BlurLevel
    {
        get => GetValue(BlurLevelProperty);
        set => SetValue(BlurLevelProperty, value);
    }

    public IBitmap? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    public float CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public readonly static StyledProperty<IBitmap?> SourceProperty =
        AvaloniaProperty.Register<BlurryImage, IBitmap?>(nameof(Source));

    public readonly static StyledProperty<Stretch> StretchProperty =
        Image.StretchProperty.AddOwner<BlurryImage>();

    public readonly static StyledProperty<StretchDirection> StretchDirectionProperty =
        Image.StretchDirectionProperty.AddOwner<BlurryImage>();

    public readonly static StyledProperty<float> BlurLevelProperty =
        AvaloniaProperty.Register<BlurryImage, float>(nameof(BlurLevel), 16);

    public static readonly StyledProperty<float> CornerRadiusProperty =
        AvaloniaProperty.Register<BlurryImage, float>(nameof(CornerRadius));

    public static readonly StyledProperty<SKImage?> SkImageProperty = AvaloniaProperty.Register<BlurryImage, SKImage?>(
        "SkImage");

    public SKImage? SkImage
    {
        get => GetValue(SkImageProperty);
        set => SetValue(SkImageProperty, value);
    }

    private BlurImageRender? _render;
}