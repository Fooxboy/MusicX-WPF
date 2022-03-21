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
            WPFUI.Appearance.Background.Remove(windowHandle);

            var appTheme = WPFUI.Appearance.Theme.GetAppTheme();
            var systemTheme = WPFUI.Appearance.Theme.GetSystemTheme();
            WPFUI.Appearance.Theme.Set(
            WPFUI.Appearance.ThemeType.Light,     // Theme type
            WPFUI.Appearance.BackgroundType.Mica, // Background type
            true                                  // Whether to change accents automatically
            );

            WPFUI.Appearance.Background.Apply(windowHandle, WPFUI.Appearance.BackgroundType.Mica);

           var res = WPFUI.Appearance.Theme.IsAppMatchesSystem();
        }
    }
}
