using System;
using System.Windows;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для NoInternetWindow.xaml
    /// </summary>
    public partial class NoInternetWindow : Window
    {
        public NoInternetWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var start = new StartingWindow(Array.Empty<string>());

            start.Show();

            this.Close();
        }
    }
}
