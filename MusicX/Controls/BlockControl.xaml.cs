using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Controls.Blocks;
using MusicX.Core.Models;
using MusicX.Services;
using MusicX.ViewModels.Controls;
using Wpf.Ui.Controls;
using Button = MusicX.Core.Models.Button;
using NavigationService = MusicX.Services.NavigationService;
using TextBlock = System.Windows.Controls.TextBlock;

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
