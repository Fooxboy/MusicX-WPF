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

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для LongreadsSliderBlockControl.xaml
    /// </summary>
    public partial class LongreadsSliderBlockControl : UserControl
    {

        public List<Longread> Longreads { get; set; }
        public LongreadsSliderBlockControl()
        {
            InitializeComponent();
            this.Loaded += LongreadsSliderBlockControl_Loaded;
        }

        private void LongreadsSliderBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(var longread in Longreads)
            {
                ListAllLongreads.Children.Add(new LongreadControl() { Height = 170, Width = 350, Margin = new Thickness(0, 0, 10, 0), Longread = longread });
            }
        }
    }
}
