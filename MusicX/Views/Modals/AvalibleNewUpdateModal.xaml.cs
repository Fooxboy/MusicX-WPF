using MusicX.Core.Models.Github;
using MusicX.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MusicX.Views.Modals
{
    /// <summary>
    /// Логика взаимодействия для AvalibleNewUpdateModal.xaml
    /// </summary>
    public partial class AvalibleNewUpdateModal : Page
    {
        public AvalibleNewUpdateModal()
        {
            InitializeComponent();
            this.Loaded += AvalibleNewUpdateModal_Loaded;
        }

        private void AvalibleNewUpdateModal_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Release release)
                return;
            OldVersion.Text = StaticService.Version;
            NewVersion.Text = release.TagName;
            Changelog.Text = release.Body;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = AppDomain.CurrentDomain.BaseDirectory + "\\MusicX.Updater.exe",
                UseShellExecute = true
            });

            Application.Current.Shutdown();
        }
    }
}
