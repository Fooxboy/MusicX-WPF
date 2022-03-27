using MusicX.Core.Models.Github;
using MusicX.Services;
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

namespace MusicX.Views.Modals
{
    /// <summary>
    /// Логика взаимодействия для AvalibleNewUpdateModal.xaml
    /// </summary>
    public partial class AvalibleNewUpdateModal : Page
    {
        private readonly Services.NavigationService navigationService;
        private readonly Release release;
        public AvalibleNewUpdateModal(Services.NavigationService navigationService, Release release)
        {
            InitializeComponent();
            this.navigationService = navigationService;
            this.release = release;
            this.Loaded += AvalibleNewUpdateModal_Loaded;
        }

        private void AvalibleNewUpdateModal_Loaded(object sender, RoutedEventArgs e)
        {
            OldVersion.Text = StaticService.Version;
            NewVersion.Text = release.Name;
            Changelog.Text = release.Body;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            navigationService.CloseModal();
        }
    }
}
