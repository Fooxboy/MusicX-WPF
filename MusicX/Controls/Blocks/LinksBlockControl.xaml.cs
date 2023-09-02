using MusicX.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для LinksBlockControl.xaml
    /// </summary>
    public partial class LinksBlockControl : UserControl
    {
        public LinksBlockControl()
        {
            InitializeComponent();
            this.Loaded += LinksBlockControl_Loaded;
        }

        private void LinksBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            LinksBlock.Visibility = Visibility.Visible;

            if (block.Layout.Name == "list")
            {
                foreach (var link in block.Links)
                {
                    ListLinks.Children.Add(new LinkControl() { Height = 80, Width = 300, Link = link, FullLink = true, Margin = new Thickness(0, 0, 10, 10) });
                }

            }
            else
            {
                foreach (var link in block.Links)
                {
                    ListLinksRec.Children.Add(new LinkControl() { Height = 140, Width = 140, Link = link, Margin = new Thickness(0, 0, 10, 0) });
                }
            }
        }
    }
}
