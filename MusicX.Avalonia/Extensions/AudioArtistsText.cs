using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Extensions;

public class AudioArtistsText
{
    public static readonly AttachedProperty<CatalogAudio> AudioProperty =
        AvaloniaProperty.RegisterAttached<AudioArtistsText, TextBlock, CatalogAudio>("Audio");

    public static readonly AttachedProperty<IDataTemplate> ArtistTemplateProperty =
        AvaloniaProperty.RegisterAttached<AudioArtistsText, TextBlock, IDataTemplate>("ArtistTemplate");

    public static void SetArtistTemplate(TextBlock obj, IDataTemplate value) => obj.SetValue(ArtistTemplateProperty, value);
    public static IDataTemplate GetArtistTemplate(TextBlock obj) => obj.GetValue(ArtistTemplateProperty);

    static AudioArtistsText()
    {
        AudioProperty.Changed.Subscribe(OnAudioChanged);
    }

    private static void OnAudioChanged(AvaloniaPropertyChangedEventArgs<CatalogAudio> args)
    {
        if (args.Sender is not TextBlock textBlock)
            return;

        var audio = args.NewValue.Value;

        if (audio.MainArtists is null)
        {
            textBlock.Text = audio.Artist;
            return;
        }

        textBlock.Inlines = new();
        var template = GetArtistTemplate(textBlock);

        IEnumerable<Inline> AddArtist(CatalogMainArtist artist, bool comma)
        {
            var control = template.Build(artist)!;
            control.DataContext = artist;
            var artistInline = new InlineUIContainer(control);
            if (comma)
                return new Inline[]
                {
                    artistInline,
                    new Run(", ")
                };
            
            return new[] { artistInline };
        }

        textBlock.Inlines.AddRange(audio.MainArtists.SelectMany((b, i) => AddArtist(b, i + 1 < audio.MainArtists.Count)));
        
        if (audio.FeaturedArtists is null)
            return;
        
        textBlock.Inlines.Add(new Run(" feat. ")
        {
            FontWeight = FontWeight.Light
        });

        textBlock.Inlines.AddRange(audio.FeaturedArtists.SelectMany((b, i) => AddArtist(b, i + 1 < audio.FeaturedArtists.Count)));
    }

    public static void SetAudio(TextBlock obj, CatalogAudio value) => obj.SetValue(AudioProperty, value);
    public static CatalogAudio GetAudio(TextBlock obj) => obj.GetValue(AudioProperty);
}