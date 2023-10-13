using MusicX.Controls;
using MusicX.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для BoomProfileView.xaml
    /// </summary>
    public partial class BoomProfileView : Page, IMenuPage
    {
        public BoomProfileView()
        {
            InitializeComponent();
            this.Loaded += BoomProfileView_Loaded;
        }

        private async void BoomProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is BoomProfileViewModel model)
            {
                await model.OpenProfile();
            }
        }

        public string MenuTag { get; set; }

        private async void TagSelected(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(500);
            if (this.DataContext is BoomProfileViewModel model)
            {
                await model.TagSelected();
            }
        }

        private async void ArtistSelected(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(500);
            if (this.DataContext is BoomProfileViewModel model)
            {
                await model.ArtistSelected();
            }
        }
    }
}
