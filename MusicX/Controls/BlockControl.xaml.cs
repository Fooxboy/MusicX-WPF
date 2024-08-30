using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Services;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для BlockControl.xaml
    /// </summary>
    public partial class BlockControl : UserControl
    {
        private readonly NavigationService navigationService;
        public BlockControl()
        {
            InitializeComponent();

            navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            this.Unloaded += BlockControl_Unloaded;
        }

        private void BlockControl_Unloaded(object sender, RoutedEventArgs e)
        {
            /*BlocksPanel.Children.Clear();*/
        }

        public static readonly DependencyProperty ArtistProperty = DependencyProperty.Register(
            nameof(Artist), typeof(Artist), typeof(BlockControl));

        public Artist? Artist
        {
            get => (Artist)GetValue(ArtistProperty);
            set => SetValue(ArtistProperty, value);
        }
    }
}
