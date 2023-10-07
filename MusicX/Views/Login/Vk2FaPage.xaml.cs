using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicX.Views.Login;

public partial class Vk2FaPage : UserControl
{
    public Vk2FaPage()
    {
        InitializeComponent();
    }

    [GeneratedRegex("\\d*")]
    private static partial Regex AllowedValuesRegex();
    
    private void CodeBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !AllowedValuesRegex().IsMatch(e.Text);
    }
}