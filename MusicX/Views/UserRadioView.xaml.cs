using MusicX.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
