using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для VideosSliderBlockControl.xaml
    /// </summary>
    public partial class VideosSliderBlockControl : UserControl
    {
        public static readonly DependencyProperty ShowFullProperty = DependencyProperty.Register(
            nameof(ShowFull), typeof(bool), typeof(VideosSliderBlockControl));

        public bool ShowFull
        {
            get => (bool)GetValue(ShowFullProperty);
            set => SetValue(ShowFullProperty, value);
        }

        public VideosSliderBlockControl()
        {
            InitializeComponent();
        }
    }
}
