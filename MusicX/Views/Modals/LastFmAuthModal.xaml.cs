using System.Windows;
using System.Windows.Controls;
using MusicX.ViewModels.Modals;

namespace MusicX.Views.Modals;

public partial class LastFmAuthModal : Page
{
    public LastFmAuthModal()
    {
        InitializeComponent();
    }

    private async void LastFmAuthModal_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is LastFmAuthModalViewModel viewModel)
            await viewModel.OpenAuthPageAsync();
    }
}