using MusicX.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicX.Controls;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для VKMixView.xaml
    /// </summary>
    public partial class VKMixView : Page, IMenuPage
    {
        public VKMixView()
        {
            InitializeComponent();

            this.Loaded += VKMixView_Loaded;
        }

        private async void VKMixView_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.DataContext is VKMixViewModel model)
            {
                await model.OpenedMixesAsync();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void ArtistSelected(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(500);
            if (this.DataContext is VKMixViewModel model)
            {
                await model.ArtistSelected();
            }
        }

        public string MenuTag { get; set; }

        private async void TagSelected(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(500);
            if (this.DataContext is VKMixViewModel model)
            {
                await model.TagSelected();
            }
        }
    }
}
