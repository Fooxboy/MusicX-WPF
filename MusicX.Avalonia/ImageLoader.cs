using System.Net;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Media.Imaging;
using MusicX.Avalonia.Controls;
using ReactiveUI;

namespace MusicX.Avalonia;

public class ImageLoader
{
    private static readonly HttpClient Client = new()
    {
        DefaultRequestVersion = HttpVersion.Version30,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
    };

    static ImageLoader()
    {
        SourceProperty.Changed.Subscribe(OnSourceChanged);
    }

    private static void OnSourceChanged(AvaloniaPropertyChangedEventArgs<string?> obj)
    {
        if (!string.IsNullOrEmpty(obj.NewValue.GetValueOrDefault()))
            RxApp.MainThreadScheduler.ScheduleAsync(((BlurryImage)obj.Sender, obj.NewValue.Value), ProvideBlurryImageAsync!);
    }

    public static readonly AttachedProperty<string?> SourceProperty =
        AvaloniaProperty.RegisterAttached<ImageLoader, BlurryImage, string?>("Source");

    public static void SetSource(BlurryImage obj, string? value) => obj.SetValue(SourceProperty, value);

    private static async Task ProvideBlurryImageAsync(IScheduler scheduler, (BlurryImage Brush, string Url) state, CancellationToken token)
    {
        using var response = await Client.GetAsync(state.Url, token);
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        state.Brush.Source = new Bitmap(stream);
    }

    public static string? GetSource(BlurryImage obj) => obj.GetValue(SourceProperty);
}