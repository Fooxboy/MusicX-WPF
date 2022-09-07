using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для RecommendedPlaylistsBlockControl.xaml
    /// </summary>
    public partial class RecommendedPlaylistsBlockControl : UserControl
    {
        public static readonly DependencyProperty ShowFullProperty = DependencyProperty.Register(
            nameof(ShowFull), typeof(bool), typeof(RecommendedPlaylistsBlockControl));

        public bool ShowFull
        {
            get => (bool)GetValue(ShowFullProperty);
            set => SetValue(ShowFullProperty, value);
        }
        public RecommendedPlaylistsBlockControl()
        {
            InitializeComponent();
        }
    }
}
