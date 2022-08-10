using System.Windows;
using Wpf.Ui.Common;
namespace MusicX.Controls;

/// <summary>
/// Lets look for things and other stuff.
/// </summary>
public class SearchBox : Wpf.Ui.Controls.AutoSuggestBox
{
    /// <summary>
    /// Property override for <see cref="Wpf.Ui.Controls.TextBox.Icon"/>.
    /// </summary>
    // Static constructor.
    static SearchBox()
    {
        FrameworkPropertyMetadata newIconMetadata = new(
            defaultValue: SymbolRegular.Search24);

        IconProperty.OverrideMetadata(
            forType: typeof(SearchBox),
            typeMetadata: newIconMetadata);
    }
}
