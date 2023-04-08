using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MusicX.Avalonia.Rendering;
using ReactiveUI;

namespace MusicX.Avalonia.Controls;

public class BlurryImage : Control
{
    private Rect _srcRect;
    private Rect _dstRect;

    static BlurryImage()
    {
        AffectsRender<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty, CornerRadiusProperty);
        AffectsMeasure<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty, CornerRadiusProperty);
        AffectsArrange<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty, CornerRadiusProperty);

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

    private void BoundsChanged(object? obj)
    {
        if (Source is null)
            return;
        var viewPort = new Rect(Bounds.Size);
        var sourceSize = Source.Size;

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
        var source = Source;
        var result = new Size();

        if (source != null)
        {
            result = Stretch.CalculateSize(availableSize, source.Size, StretchDirection);
        }

        return result;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = Source;

        if (source == null) return new Size();
        var sourceSize = source.Size;
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

    private BlurImageRender? _render;
}