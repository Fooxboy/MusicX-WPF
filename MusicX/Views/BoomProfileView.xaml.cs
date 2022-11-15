using MusicX.Controls;
using MusicX.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
