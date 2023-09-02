using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Shared.ListenTogether.Radio;
using MusicX.ViewModels;
using MusicX.Views;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для UserRadioBlockControl.xaml
    /// </summary>
    public partial class UserRadioBlockControl : UserControl
    {
        public UserRadioBlockControl()
        {
            this.DataContext = this;
            this.Loaded += UserRadioBlockControl_Loaded;
            InitializeComponent();
        }

        private void UserRadioBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            if (block.Stations is null)
            {
                block.Stations = new List<Station>();
            }

            foreach (var station in block.Stations)
            {
                ListStations.Items.Add( new UserStationControl() { Station = station});
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var navigation = StaticService.Container.GetRequiredService<NavigationService>();

            var radioView = new UserRadioView();
            radioView.DataContext = new UserRadioViewModel();
            navigation.OpenExternalPage(radioView);
        }
    }
}
