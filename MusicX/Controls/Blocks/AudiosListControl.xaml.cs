using MusicX.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для AudiosListControl.xaml
    /// </summary>
    public partial class AudiosListControl : UserControl
    {
        public AudiosListControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty AudiosProperty =
         DependencyProperty.Register("Audios", typeof(IEnumerable<Audio>),
             typeof(AudiosListControl), new PropertyMetadata(Enumerable.Empty<Audio>()));

        public ICollection<Audio> Audios
        {
            get => (List<Audio>)GetValue(AudiosProperty);
            set => SetValue(AudiosProperty, value);
        }

        public static readonly DependencyProperty ShowChartProperty = DependencyProperty.Register(
            nameof(ShowChart), typeof(bool), typeof(AudiosListControl), new PropertyMetadata(default(bool)));

        public bool ShowChart
        {
            get => (bool)GetValue(ShowChartProperty);
            set => SetValue(ShowChartProperty, value);
        }
    }
}
