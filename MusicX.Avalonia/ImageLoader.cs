using System.Collections.Concurrent;
using System.Net;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MusicX.Avalonia.Controls;
using ReactiveUI;
using SkiaSharp;

namespace MusicX.Avalonia;

public class ImageLoader
{
    private static readonly ConcurrentDictionary<string, IBitmap> Bitmaps = new();
    private static readonly ConcurrentDictionary<string, SKBitmap> Images = new();

    private static readonly HttpClient Client = new()
    {
        DefaultRequestVersion = HttpVersion.Version30,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
    };

    static ImageLoader()
    {
        BlurrySourceProperty.Changed.Subscribe(OnBlurrySourceChanged);
        BrushSourceProperty.Changed.Subscribe(OnSourceChanged);
    }

    private static void OnBlurrySourceChanged(AvaloniaPropertyChangedEventArgs<string?> obj)
    {
        if (!string.IsNullOrEmpty(obj.NewValue.GetValueOrDefault()))
            RxApp.TaskpoolScheduler.ScheduleAsync(((BlurryImage)obj.Sender, obj.NewValue.Value!), ProvideBlurryImageAsync);
        else ((BlurryImage)obj.Sender).Reset();
    }

    private static async Task ProvideBlurryImageAsync(IScheduler scheduler, (BlurryImage image, string Url) state,
                                                      CancellationToken token)
    {
        if (!Images.TryGetValue(state.Url, out var image))
        {
            await using var stream = await Client.GetStreamAsync(state.Url, token);
            image = SKBitmap.Decode(stream);
            image.SetImmutable();
            Images.TryAdd(state.Url, image);
        }
        
        state.image.Load(image);
    }

    private static void OnSourceChanged(AvaloniaPropertyChangedEventArgs<string?> obj)
    {
        if (!string.IsNullOrEmpty(obj.NewValue.GetValueOrDefault()))
            RxApp.TaskpoolScheduler.ScheduleAsync((obj.Sender, obj.Sender switch
            {
                BlurryImage => BlurryImage.SourceProperty,
                ImageBrush => ImageBrush.SourceProperty,
                _ => throw new ArgumentOutOfRangeException()
            }, obj.NewValue.Value), ProvideImageAsync!);
    }

    public static readonly AttachedProperty<string?> BlurrySourceProperty =
        AvaloniaProperty.RegisterAttached<ImageLoader, BlurryImage, string?>("Source");

    public static readonly AttachedProperty<string?> BrushSourceProperty =
        AvaloniaProperty.RegisterAttached<ImageLoader, ImageBrush, string?>("BrushSource");

    public static void SetBrushSource(ImageBrush obj, string? value) => obj.SetValue(BrushSourceProperty, value);
    public static string? GetBrushSource(ImageBrush obj) => obj.GetValue(BrushSourceProperty);

    public static void SetBlurrySource(BlurryImage obj, string? value) => obj.SetValue(BlurrySourceProperty, value);

    private static async Task ProvideImageAsync(IScheduler scheduler, (AvaloniaObject Sender, StyledProperty<IBitmap?> Property, string Url) state, CancellationToken token)
    {
        if (!Bitmaps.TryGetValue(state.Url, out var bitmap))
        {
            using var response = await Client.GetAsync(state.Url, token);
            await using var stream = await response.Content.ReadAsStreamAsync(token);
            bitmap = new Bitmap(stream);
            Bitmaps.TryAdd(state.Url, bitmap);
        }
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            state.Sender.SetValue(state.Property, bitmap);
        });
    }

    public static string? GetBlurrySource(BlurryImage obj) => obj.GetValue(BlurrySourceProperty);
}