using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace MusicX.Avalonia.Controls;

public class LinkButton : Button
{
    public static readonly StyledProperty<string> ImageUrlProperty = AvaloniaProperty.Register<LinkButton, string>(
        "ImageUrl");

    public string ImageUrl
    {
        get => GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<LinkButton, string>(
        "Title");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> SubtitleProperty = AvaloniaProperty.Register<LinkButton, string>(
        "Subtitle");

    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
}