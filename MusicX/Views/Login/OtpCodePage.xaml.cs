using System.Text.RegularExpressions;
using System.Windows.Input;
using MusicX.ViewModels.Login;
using Wpf.Ui.Controls;

namespace MusicX.Views.Login;

public partial class OtpCodePage : INavigableView<OtpCodeViewModel>
{
    public OtpCodePage(OtpCodeViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        
        InitializeComponent();
    }
    
    [GeneratedRegex("\\d*")]
    private static partial Regex AllowedValuesRegex();

    private void CodeBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !AllowedValuesRegex().IsMatch(e.Text);
    }

    public OtpCodeViewModel ViewModel { get; }
}