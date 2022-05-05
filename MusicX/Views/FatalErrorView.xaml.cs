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
using System.Windows.Shapes;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для FatalErrorView.xaml
    /// </summary>
    public partial class FatalErrorView : Window
    {
        private readonly Exception exInfo;
        public FatalErrorView(Exception ex)
        {
            InitializeComponent();

            exInfo = ex;

            this.Loaded += FatalErrorView_Loaded;
        }

        private void FatalErrorView_Loaded(object sender, RoutedEventArgs e)
        {
            dataError.Text = exInfo.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var start = new StartingWindow();

            start.Show();

            this.Close();
        }
    }
}
