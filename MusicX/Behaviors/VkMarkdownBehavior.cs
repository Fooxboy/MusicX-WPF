using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
namespace MusicX.Behaviors;

public static class VkMarkdownBehavior
{
    private static readonly Regex MarkdownRegex = new Regex(@"\[(?<link>[^\|]+)\|(?<text>[^\]]+)\]", RegexOptions.Compiled);

    public static readonly DependencyProperty MarkdownProperty = DependencyProperty.RegisterAttached(
        "Markdown", typeof(string), typeof(VkMarkdownBehavior), new PropertyMetadata(MarkdownChanged));

    public static void SetMarkdown(DependencyObject element, string value)
    {
        element.SetValue(MarkdownProperty, value);
    }

    public static string GetMarkdown(DependencyObject element)
    {
        return (string)element.GetValue(MarkdownProperty);
    }
    
    private static void MarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock block)
            return;

        var text = e.NewValue as string;

        if (string.IsNullOrEmpty(text))
        {
            block.Inlines.Clear();
            return;
        }

        var index = 0;
        foreach (Match match in MarkdownRegex.Matches(text))
        {
            var link = match.Groups["link"].Value;

            if (!link.StartsWith("https://"))
                link = $"https://vk.com/{link}";
            // dont allow external links
            else if (!link.StartsWith("https://vk.com"))
                link = "https://vk.com";

            block.Inlines.Add(text[index..match.Index]);
            
            var hyperlink = new Hyperlink
            {
                NavigateUri = new(link, UriKind.Absolute),
                Inlines = { match.Groups["text"].Value }
            };
            hyperlink.RequestNavigate += HyperlinkOnRequestNavigate;
            
            block.Inlines.Add(hyperlink);

            index = match.Index + match.Length;
        }
        
        // add remaining text
        block.Inlines.Add(text[index..]);
    }
    private static void HyperlinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;

        Process.Start(new ProcessStartInfo(e.Uri.ToString())
        {
            UseShellExecute = true
        });
    }
}
