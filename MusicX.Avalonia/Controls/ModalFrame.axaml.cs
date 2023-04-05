using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FluentAvalonia.UI.Controls;

namespace MusicX.Avalonia.Controls;

public class ModalFrame : Frame
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ModalFrame, string?>(nameof(Title));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}