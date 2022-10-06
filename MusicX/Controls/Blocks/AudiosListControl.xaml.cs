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

        public IEnumerable<Audio> Audios
        {
            get => (List<Audio>)GetValue(AudiosProperty);
            set => SetValue(AudiosProperty, value);
        }
    }
}
