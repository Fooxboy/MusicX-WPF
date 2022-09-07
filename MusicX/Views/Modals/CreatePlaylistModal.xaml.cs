using MusicX.Core.Models;
using MusicX.ViewModels.Modals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicX.Views.Modals
{
    /// <summary>
    /// Логика взаимодействия для CreatePlaylistModal.xaml
    /// </summary>
    public partial class CreatePlaylistModal : Page
    {
        public CreatePlaylistModal()
        {
            InitializeComponent();
        }

        private void ListViewItem_Drop(object sender, DragEventArgs e)
        {
            var source = (Audio)e.Data.GetData(typeof(Audio))!;
            var target = (Audio)((ListViewItem)sender).DataContext;

            ((CreatePlaylistModalViewModel)DataContext).MoveTracks(source, target);
            
        }

        private void SymbolIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement { TemplatedParent: FrameworkElement { TemplatedParent: ListViewItem item } })
                DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
        }
    }
}
