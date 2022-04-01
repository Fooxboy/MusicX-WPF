using DryIoc;
using MusicX.Core.Models;
using MusicX.Services;
using NLog;
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
    /// Логика взаимодействия для TitleBlockControl.xaml
    /// </summary>
    public partial class TitleBlockControl : UserControl
    {
        public Block Block { get; set; }
        public TitleBlockControl()
        {
            InitializeComponent();
            Loaded += TitleBlockControl_Loaded;
        }

        private void TitleBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            Buttons.SelectionChanged += ButtonsComboBox_SelectionChanged;

            if (Block.Layout.Name == "header_compact")
            {
                Title.Opacity = 0.5;
                Title.FontSize = 15;
            }

            Title.Text = Block.Layout.Title;

            if (Block.Badge != null)
            {
                BadgeHeader.Text = Block.Badge.Text;
                BadgeHeader.Visibility = Visibility.Visible;
            }

            if (Block.Buttons != null && Block.Buttons.Count > 0)
            {
                if (Block.Buttons[0].Options.Count > 0)
                {
                    Buttons.Visibility = Visibility.Visible;
                    MoreButton.Visibility = Visibility.Collapsed;
                    foreach (var option in Block.Buttons[0].Options)
                    {
                        Buttons.Items.Add(new TextBlock() { Text = option.Text });
                    }
                    Buttons.SelectedIndex = 0;
                    return;
                }
                else
                {
                    MoreButton.Visibility = Visibility.Visible;

                    MoreButton.Content = Block.Buttons[0].Title;

                    return;

                }
            }
            else
            {
                
                if(Block.Actions.Count > 0)
                {

                    if (Block.Actions[0].Options.Count > 0)
                    {
                        Buttons.Visibility = Visibility.Visible;
                        MoreButton.Visibility = Visibility.Collapsed;
                        foreach (var option in Block.Actions[0].Options)
                        {
                            Buttons.Items.Add(new TextBlock() { Text = option.Text });
                        }
                        Buttons.SelectedIndex = 0;
                        return;
                    }
                    else
                    {
                        MoreButton.Visibility = Visibility.Visible;

                        MoreButton.Content = Block.Actions[0].Title;

                        return;

                    }

                }

                return;
            }
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

                if (Block.Actions.Count > 0)
                {
                    var bnt = Block.Actions[0];

                    await navigationService.OpenSection(bnt.SectionId, true);
                    return;
                }

                var button = Block.Buttons[0];

                await navigationService.OpenSection(button.SectionId, true);
            }
            catch (Exception ex)
            {
                var logger = StaticService.Container.Resolve<Logger>();

                logger.Error(ex, ex.Message);
            }


        }

        private async void ButtonsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;

                var current = comboBox.SelectedIndex;

                if (current == 0) return;

                OptionButton option;
                if(Block.Buttons != null)
                {
                    option = Block.Buttons[0].Options[current];

                }else
                {
                    option = Block.Actions[0].Options[current];

                }

                var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

                await navigationService.ReplaceBlock(option.ReplacementId);
            }
            catch (Exception ex)
            {
                var logger = StaticService.Container.Resolve<Logger>();

                logger.Error(ex, ex.Message);
            }


            //throw new NotImplementedException();
        }
    }
}
