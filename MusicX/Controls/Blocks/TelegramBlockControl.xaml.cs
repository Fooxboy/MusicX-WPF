using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для TelegramBlockControl.xaml
    /// </summary>
    public partial class TelegramBlockControl : UserControl
    {
        public TelegramBlockControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://t.me/MusicXPlayer",
                UseShellExecute = true
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://t.me/+lO37psdwX2s3NjZi",
                UseShellExecute = true
            });
        }
    }
}
