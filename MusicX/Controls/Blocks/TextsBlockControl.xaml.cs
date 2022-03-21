using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для TextsBlockControl.xaml
    /// </summary>
    public partial class TextsBlockControl : UserControl
    {
        public Block Block { get; set; }
        public TextsBlockControl()
        {
            InitializeComponent();
            this.Loaded += TextsBlockControl_Loaded;
        }

        private void TextsBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
          TextsValue.Text = Block.Texts[0].Value;

        }
    }
}
