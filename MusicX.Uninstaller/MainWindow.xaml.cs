using System;
using System.Collections.Generic;
using System.IO;
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

namespace MusicX.Uninstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                var unistall = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                unistall.DeleteSubKeyTree("MusicX");
            }catch (Exception ex)
            {
               
            }
            
            try
            {
                if(File.Exists("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs" + "\\Music X.lnk"))
                {
                    File.Delete("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs" + "\\Music X.lnk");

                }

                if(File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Music X.lnk"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Music X.lnk");

                }

            }
            catch (Exception ex)
            {
                

            }


            var currentPath = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo di = new DirectoryInfo(currentPath);

            try
            {
                di.Delete(true);

            }catch (Exception ex)
            {

            }

            CloseButton.Visibility = Visibility.Visible;
            TextDelete.Text = "Music X был удален с Вашего компьютера";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
