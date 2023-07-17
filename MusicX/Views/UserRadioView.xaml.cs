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
    /// Логика взаимодействия для UserRadioView.xaml
    /// </summary>
    public partial class UserRadioView : Page
    {
        public UserRadioView()
        {
            InitializeComponent();

            this.Loaded += UserRadioView_Loaded;
        }

        private async void UserRadioView_Loaded(object sender, RoutedEventArgs e)
        {
            await (this.DataContext as UserRadioViewModel).LoadData();
        }

        private void RadioCard_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as UIElement).Opacity = 0.7;
        }

        private void RadioCard_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as UIElement).Opacity = 1;

        }
    }
}
