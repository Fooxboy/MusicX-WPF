using System.Windows;

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
            //РАБОТАЕТ!!!! (ne rabotaet)
            /*IntPtr windowHandle = new WindowInteropHelper(this).Handle;
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

           var res = Wpf.Ui.Appearance.Theme.IsAppMatchesSystem();*/
        }
    }
}
