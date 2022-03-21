using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для CuratorBannerBlockControl.xaml
    /// </summary>
    public partial class CuratorBannerBlockControl : UserControl
    {
        public Block Block { get; set; }
        public CuratorBannerBlockControl()
        {
            InitializeComponent();
            this.Loaded += CuratorBannerBlockControl_Loaded;
        }

        private void CuratorBannerBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            CuratorBannerImage.ImageSource = new BitmapImage(new Uri(Block.Curators[0].Photo[2].Url));
            CuratorText.Text = Block.Curators[0].Name;
            CuratorDescription.Text = Block.Curators[0].Description;
        }
    }
}
