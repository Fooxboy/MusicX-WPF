using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для ListTracks.xaml
    /// </summary>
    public partial class ListTracks : UserControl
    {
        public ListTracks()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TracksProperty =
         DependencyProperty.Register(nameof(Tracks), typeof(IList<Audio>), typeof(ListTracks), new PropertyMetadata(Array.Empty<Audio>()));
        
        public static readonly DependencyProperty ShowChartProperty =
          DependencyProperty.Register(nameof(ShowChart), typeof(bool), typeof(ListPlaylists), new PropertyMetadata(false));

        public bool ShowChart
        {
            get => (bool)GetValue(ShowChartProperty);
            set => SetValue(ShowChartProperty, value);
        }

        public IList<Audio> Tracks
        {
            get => (IList<Audio>)GetValue(TracksProperty);
            set => SetValue(TracksProperty, value);
        }
    }
}
