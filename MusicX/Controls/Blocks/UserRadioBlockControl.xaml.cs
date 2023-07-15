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
    /// Логика взаимодействия для UserRadioBlockControl.xaml
    /// </summary>
    public partial class UserRadioBlockControl : UserControl
    {
        public UserRadioBlockControl()
        {
            this.Loaded += UserRadioBlockControl_Loaded;
            InitializeComponent();
        }

        private void UserRadioBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public Block Block { get; set; }

    }
}
