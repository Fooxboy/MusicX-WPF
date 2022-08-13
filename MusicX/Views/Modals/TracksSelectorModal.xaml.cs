using MusicX.ViewModels.Modals;
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

namespace MusicX.Views.Modals
{
    /// <summary>
    /// Логика взаимодействия для TracksSelectorModal.xaml
    /// </summary>
    public partial class TracksSelectorModal : Page
    {
        public TracksSelectorModal()
        {
            this.Loaded += TracksSelectorModalLoaded;
            InitializeComponent();
        }

        private async void TracksSelectorModalLoaded(object sender, RoutedEventArgs e)
        {
           await ((TracksSelectorModalViewModel)DataContext).LoadTracksAsync();
        }

        private async void MultiSelectListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {

                //Debug.WriteLine($" ExtentHeight: {e.ExtentHeight} | VerticalOffset: {e.VerticalOffset + Window.GetWindow(this).ActualHeight }");
                if (e.ExtentHeight < e.VerticalOffset + 10)
                {
                    await ((TracksSelectorModalViewModel)DataContext).LoadTracksAsync();
                }
            }
            catch (Exception ex)
            {
              
            }
        }
    }
}
