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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для MicaTestWindow.xaml
    /// </summary>
    public partial class MicaTestWindow : Window
    {
        public MicaTestWindow()
        {
            InitializeComponent();
            this.Loaded += MicaTestWindow_Loaded;
        }

        private void MicaTestWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //РАБОТАЕТ!!!!
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            this.Background = Brushes.Transparent;
            Wpf.Ui.Appearance.Background.Remove(windowHandle);

            var appTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
            var systemTheme = Wpf.Ui.Appearance.Theme.GetSystemTheme();
            Wpf.Ui.Appearance.Theme.Apply(
            Wpf.Ui.Appearance.ThemeType.Light,     // Theme type
            Wpf.Ui.Appearance.BackgroundType.Mica, // Background type
            true                                  // Whether to change accents automatically
            );

            Wpf.Ui.Appearance.Background.Apply(windowHandle, Wpf.Ui.Appearance.BackgroundType.Mica);

           var res = Wpf.Ui.Appearance.Theme.IsAppMatchesSystem();
        }
    }
}
