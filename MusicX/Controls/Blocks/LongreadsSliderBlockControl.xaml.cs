using MusicX.Core.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
            var currentIndex = 0;
            foreach(var longread in Longreads)
            {
                if (currentIndex == 5) break;
                ListAllLongreads.Children.Add(new LongreadControl() { Height = 170, Width = 350, Margin = new Thickness(0, 0, 10, 0), Longread = longread });
                currentIndex++;
            }
        }
    }
}
