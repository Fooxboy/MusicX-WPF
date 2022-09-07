using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для FatalErrorView.xaml
    /// </summary>
    public partial class FatalErrorView : UiWindow
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
