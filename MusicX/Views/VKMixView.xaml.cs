using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Services;
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
            await Task.Delay(1000);
            if (this.DataContext is VKMixViewModel model)
            {
                await model.ArtistSelected();
            }
        }

        public string MenuTag { get; set; }
    }
}
