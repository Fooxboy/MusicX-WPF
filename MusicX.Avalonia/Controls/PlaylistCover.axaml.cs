using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;

namespace MusicX.Avalonia.Controls;

public class PlaylistCover : TemplatedControl
{
    public static readonly StyledProperty<string> ImageUrlProperty =
        AvaloniaProperty.Register<PlaylistCover, string>(nameof(ImageUrl));

    public string ImageUrl
    {
        get => GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PlaylistCover, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<PlaylistCover, string?>(nameof(Subtitle));

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<PlaylistCover, ICommand>(
        "Command");

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<PlaylistCover, object?>(
        "CommandParameter");

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}