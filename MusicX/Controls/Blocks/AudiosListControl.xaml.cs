using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для AudiosListControl.xaml
    /// </summary>
    public partial class AudiosListControl : UserControl
    {
       
        public BitmapImage BitImage { get; set; }

        public AudiosListControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty AudiosProperty =
         DependencyProperty.Register("Audios", typeof(List<Audio>),
             typeof(AudiosListControl), new PropertyMetadata(new List<Audio>(), OnFirstPropertyChanged));

        public List<Audio> Audios
        {
            get { return (List<Audio>)GetValue(AudiosProperty); }
            set
            {
                SetValue(AudiosProperty, value);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            return;
        }

        private static void OnFirstPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var audios = (List<Audio>)e.NewValue;

            var control = sender as AudiosListControl;

             if (control.ListAllTracks.Children.Count > 0) control.ListAllTracks.Children.RemoveRange(0, control.ListAllTracks.Children.Count - 1);

            foreach (var audio in audios)
            {
                control.ListAllTracks.Children.Add(new TrackControl() { BitImage = control.BitImage, ShowCard = false, Margin = new Thickness(0, 0, 0, 5), Height = 60, Audio = audio });
                control.ListAllTracks.Children.Add(new Rectangle() { Height = 1, Fill = Brushes.White, Margin = new Thickness(5, 0, 5, 5), Opacity = 0.2 });
            }
        }
    }
}
