using MusicX.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для TextsBlockControl.xaml
    /// </summary>
    public partial class TextsBlockControl : UserControl
    {
        public TextsBlockControl()
        {
            InitializeComponent();
            this.Loaded += TextsBlockControl_Loaded;
        }

        private void TextsBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is Block block)
                TextsValue.Text = block.Texts[0].Value;
        }
    }
}
