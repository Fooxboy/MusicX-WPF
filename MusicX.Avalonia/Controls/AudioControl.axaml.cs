using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Controls;

public class AudioControl : TemplatedControl
{
    public static readonly StyledProperty<CatalogAudio> AudioProperty = AvaloniaProperty.Register<AudioControl, CatalogAudio>(
        "Audio");

    public CatalogAudio Audio
    {
        get => GetValue(AudioProperty);
        set => SetValue(AudioProperty, value);
    }
}