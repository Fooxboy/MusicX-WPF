using MusicX.Core.Models;
using System;
using System.Collections.Generic;
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

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для LongreadControl.xaml
    /// </summary>
    public partial class LongreadControl : UserControl
    {

        public Longread Longread { get; set; }
        public LongreadControl()
        {
            InitializeComponent();
            this.Loaded += LongreadControl_Loaded;
        }

        private void LongreadControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.BackgrondImage.Source = new BitmapImage(new Uri(Longread.Photo.Sizes[3].Url));
            this.Title.Text = Longread.Title;


        }
    }
}
