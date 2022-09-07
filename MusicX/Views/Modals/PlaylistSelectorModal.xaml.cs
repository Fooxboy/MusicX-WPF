using MusicX.ViewModels.Modals;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Views.Modals
{
    /// <summary>
    /// Логика взаимодействия для PlaylistSelectorModal.xaml
    /// </summary>
    public partial class PlaylistSelectorModal : Page
    {

        public PlaylistSelectorModal()
        {
            this.Loaded += PlaylistSelectorModal_Loaded;
            InitializeComponent();
        }

        private async void PlaylistSelectorModal_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.DataContext is PlaylistSelectorModalViewModel vm)
            {
                await vm.LoadPlaylistsAsync();
            }
        }
    }
}
