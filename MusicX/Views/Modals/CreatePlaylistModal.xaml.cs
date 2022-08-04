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
    /// Логика взаимодействия для CreatePlaylistModal.xaml
    /// </summary>
    public partial class CreatePlaylistModal : Page
    {
        public CreatePlaylistModal(CreatePlaylistModalViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
