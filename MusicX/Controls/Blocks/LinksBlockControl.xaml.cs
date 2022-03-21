using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для LinksBlockControl.xaml
    /// </summary>
    public partial class LinksBlockControl : UserControl
    {
        public Block Block { get; set; }
        public LinksBlockControl()
        {
            InitializeComponent();
            this.Loaded += LinksBlockControl_Loaded;
        }

        private void LinksBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            LinksBlock.Visibility = Visibility.Visible;

            if (Block.Layout.Name == "list")
            {
                foreach (var link in Block.Links)
                {
                    ListLinks.Children.Add(new LinkControl() { Height = 80, Width = 300, Link = link, FullLink = true, Margin = new Thickness(0, 0, 10, 0) });
                }

            }
            else
            {
                foreach (var link in Block.Links)
                {
                    ListLinks.Children.Add(new LinkControl() { Height = 140, Width = 140, Link = link, Margin = new Thickness(0, 0, 10, 0) });
                }
            }
        }
    }
}
