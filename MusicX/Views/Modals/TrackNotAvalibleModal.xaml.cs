using MusicX.Core.Services;
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
    /// Логика взаимодействия для TrackNotAvalibleModal.xaml
    /// </summary>
    public partial class TrackNotAvalibleModal : Page
    {
        private readonly VkService vkService;
        private readonly Services.NavigationService navigationService;
        private readonly string trackCode;
        private readonly string audioId;
        public TrackNotAvalibleModal(VkService vkService, Services.NavigationService navigationService, string trackCode, string audioId)
        {
            InitializeComponent();
            this.vkService = vkService;
            this.trackCode = trackCode;
            this.audioId = audioId;
            this.navigationService = navigationService;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var res = await vkService.AudioGetRestrictionPopup(trackCode, audioId);

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Image.Source = new BitmapImage(new Uri(res.Icons[0].Url));
                        Title.Text = res.Title;
                        Description.Text = res.Text;

                        LoadingGrid.Visibility = Visibility.Collapsed;
                        ContentGrid.Visibility = Visibility.Visible;
                    });
                }catch (Exception ex)
                {
                    this.navigationService.CloseModal();
                }
                
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.navigationService.CloseModal();
        }
    }
}
