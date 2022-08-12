using MusicX.Core.Models;
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
        private readonly CreatePlaylistModalViewModel vm;
        public CreatePlaylistModal(CreatePlaylistModalViewModel viewModel)
        {
            this.vm = viewModel;
            this.DataContext = viewModel;
            InitializeComponent();
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e)
        {
            var source = (Audio)e.Data.GetData(typeof(Audio))!;
            var target = (Audio)((ListViewItem)sender).DataContext;

            this.vm.MoveTracks(source, target);
            
        }

        private void SymbolIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { TemplatedParent: FrameworkElement { TemplatedParent: ListViewItem item } })
                DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
        }
    }
}
