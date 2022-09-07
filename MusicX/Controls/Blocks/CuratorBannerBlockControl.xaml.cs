using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для CuratorBannerBlockControl.xaml
    /// </summary>
    public partial class CuratorBannerBlockControl : UserControl
    {
        public Block Block { get; set; }
        public CuratorBannerBlockControl()
        {
            this.Loaded += CuratorBannerBlockControl_Loaded;
            this.Initialized += CuratorBannerBlockControl_Initialized;
            InitializeComponent();

            
        }

        private void CuratorBannerBlockControl_Initialized(object? sender, EventArgs e)
        {

           
        }

        private void CuratorBannerBlockControl_Loaded(object sender, RoutedEventArgs e)
        {

            CuratorBannerImage.ImageSource = new BitmapImage(new Uri(Block.Curators[0].Photo[2].Url));
            CuratorText.Text = Block.Curators[0].Name;
            //CuratorDescription.Text = Block.Curators[0].Description;

            if (Block.Curators[0].IsFollowed)
            {
                ActionCuratorButton.Content = "Отписаться";
                ActionCuratorButton.Icon = Wpf.Ui.Common.SymbolRegular.DeleteDismiss20;
            }
            else
            {

                ActionCuratorButton.Content = "Подписаться";
                ActionCuratorButton.Icon = Wpf.Ui.Common.SymbolRegular.Add24;
            }

        }

        private async void ActionCuratorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vkService = StaticService.Container.GetRequiredService<VkService>();
                if (Block.Curators[0].IsFollowed)
                {
                    ActionCuratorButton.IsEnabled = false;
                    ActionCuratorButton.Content = "Секунду..";
                    ActionCuratorButton.Icon = Wpf.Ui.Common.SymbolRegular.Timer28;

                    await vkService.UnfollowCurator(Block.Curators[0].Id);

                    ActionCuratorButton.IsEnabled = true;

                    Block.Curators[0].IsFollowed = false;
                    ActionCuratorButton.Content = "Подписаться";
                    ActionCuratorButton.Icon = Wpf.Ui.Common.SymbolRegular.Add24;
                }
                else
                {
                    ActionCuratorButton.IsEnabled = false;
                    ActionCuratorButton.Content = "Секунду..";
                    ActionCuratorButton.Icon = Wpf.Ui.Common.SymbolRegular.Timer28;

                    await vkService.FollowCurator(Block.Curators[0].Id);

                    ActionCuratorButton.IsEnabled = true;
                    Block.Curators[0].IsFollowed = true;
                    ActionCuratorButton.Content = "Отписаться";
                    ActionCuratorButton.Icon = Wpf.Ui.Common.SymbolRegular.DeleteDismiss20;
                }
            }catch (Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, ex.Message);

            }

        }
    }
}
