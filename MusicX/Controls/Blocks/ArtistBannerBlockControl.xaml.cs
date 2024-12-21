using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MusicX.Helpers;
using MusicX.ViewModels;
using MusicX.ViewModels.Controls;
using MusicX.Views;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для ArtistBannerBlockControl.xaml
    /// </summary>
    public partial class ArtistBannerBlockControl : UserControl
    {
        public static readonly DependencyProperty BlockProperty = DependencyProperty.Register(
            nameof(Block), typeof(BlockViewModel), typeof(ArtistBannerBlockControl), new PropertyMetadata(BlockChangedCallback));

        public BlockViewModel Block
        {
            get => (BlockViewModel)GetValue(BlockProperty);
            set => SetValue(BlockProperty, value);
        }
        
        public ArtistBannerBlockControl()
        {
            InitializeComponent();

            ArtistText.Visibility = Visibility.Collapsed;
            ArtistBanner.Visibility = Visibility.Collapsed;
        }

        private static void BlockChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ArtistBannerBlockControl control || e.NewValue is not BlockViewModel)
                return;
            
            control.Load();
        }

        private void Load()
        {
            new Thread(()=>
            {
                
                Thread.Sleep(800);

                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ArtistText.Visibility = Visibility.Visible;
                    ArtistBanner.Visibility = Visibility.Visible;
                    var amim = (Storyboard)(this.Resources["OpenAnimation"]);
                    amim.Begin();
                });

               
            }).Start();

            ArtistBannerImage.ImageSource = new BitmapImage(new Uri(Block.Artists[0].Photo[2].Url));
            ArtistText.Text = Block.Artists[0].Name;

            var sectionViewModel = (SectionViewModel)this.FindAncestor<SectionView>()!.DataContext;

            for (var i = 0; i < Block.Buttons.Count; i++)
            {
                var action = Block.Buttons[i];
                
                var text = new TextBlock();
                var card = new CardAction()
                {
                    Margin = new Thickness(0, 10, 15, 10),
                    Content = text,
                    DataContext = new BlockButtonViewModel(action, sectionViewModel.Artist, Block),
                    IsChevronVisible = false,
                    Height = 45
                };

                card.SetBinding(ButtonBase.CommandProperty, new Binding(nameof(BlockButtonViewModel.InvokeCommand)));
                card.SetBinding(CardAction.IconProperty, new Binding(nameof(BlockButtonViewModel.Icon)));
                text.SetBinding(TextBlock.TextProperty, new Binding(nameof(BlockButtonViewModel.Text)));

                ActionsGrid.ColumnDefinitions.Add(new() {MinWidth = 170});
                card.SetValue(Grid.ColumnProperty, i);
                ActionsGrid.Children.Add(card);
            }
        }

        private void ActionArtistButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var amim = (Storyboard)(this.Resources["OpenAnimation"]);
            amim.Begin();
        }
    }
}
