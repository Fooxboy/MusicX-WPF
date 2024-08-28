using MusicX.Core.Models;
using System.Windows;
using System.Windows.Controls;
using MusicX.ViewModels;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для LinksBlockControl.xaml
    /// </summary>
    public partial class LinksBlockControl : UserControl
    {
        public static readonly DependencyProperty BlockProperty = DependencyProperty.Register(
            nameof(Block), typeof(BlockViewModel), typeof(LinksBlockControl), new(default(Block), BlockChanged));

        private static void BlockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LinksBlockControl control || e.NewValue is not BlockViewModel block)
                return;
            
            if (block.Layout?.Name == "list")
            {
                foreach (var link in block.Links)
                {
                    control.ListLinks.Children.Add(new LinkControl() { Height = 80, Width = 300, Link = link, FullLink = true, Margin = new Thickness(0, 0, 10, 10) });
                }
            }
            else
            {
                foreach (var link in block.Links)
                {
                    control.ListLinksRec.Children.Add(new LinkControl() { Height = 140, Width = 140, Link = link, Margin = new Thickness(0, 0, 10, 0) });
                }
            }
        }

        public BlockViewModel Block
        {
            get => (BlockViewModel)GetValue(BlockProperty);
            set => SetValue(BlockProperty, value);
        }
        
        public LinksBlockControl()
        {
            InitializeComponent();
        }
    }
}
