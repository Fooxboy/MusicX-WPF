using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using MusicX.ViewModels;
using MusicX.Views;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для UserRadioBlockControl.xaml
    /// </summary>
    public partial class UserRadioBlockControl : UserControl
    {
        public static readonly DependencyProperty BlockProperty = DependencyProperty.Register(
            nameof(Block), typeof(BlockViewModel), typeof(UserRadioBlockControl), new PropertyMetadata(BlockChangedCallback));

        private static void BlockChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UserRadioBlockControl control || e.NewValue is not BlockViewModel)
                return;
            
            control.Load();
        }

        public BlockViewModel Block
        {
            get => (BlockViewModel)GetValue(BlockProperty);
            set => SetValue(BlockProperty, value);
        }
        
        public UserRadioBlockControl()
        {
            InitializeComponent();
        }

        private void Load()
        {
            foreach (var station in Block.Stations)
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
