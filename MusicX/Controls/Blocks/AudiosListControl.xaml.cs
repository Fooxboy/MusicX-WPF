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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

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
