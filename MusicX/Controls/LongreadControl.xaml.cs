using MusicX.Core.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
