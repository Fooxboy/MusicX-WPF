using MusicX.Core.Models;
using MusicX.Services;
using NLog;
using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для TitleBlockControl.xaml
    /// </summary>
    public partial class TitleBlockControl : UserControl
    {
        public TitleBlockControl()
        {
            InitializeComponent();
            DataContextChanged += TitleBlockControl_Loaded;
        }

        private void TitleBlockControl_Loaded(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is not Block block)
                return;

            if(RootWindow.WinterTheme)
            {
                var r = new Random();
                var i = r.Next(0, 3);
                if (i == 1)
                {
                    Shapka.Visibility = Visibility.Visible;
                }
            }
           
            Buttons.SelectionChanged += ButtonsComboBox_SelectionChanged;

            if (block.Layout.Name == "header_compact")
            {
                Title.Opacity = 0.5;
                Title.FontSize = 15;
            }

            Title.Text = block.Layout.Title;

            if (block.Badge != null)
            {
                BadgeHeader.Text = block.Badge.Text;
                BadgeHeader.Visibility = Visibility.Visible;
            }

            if (block.Buttons != null && block.Buttons.Count > 0) //ios
            {
                if (block.Buttons[0].Options.Count > 0)
                {
                    ButtonsGrid.Visibility = Visibility.Visible;
                    TitleButtons.Text = block.Buttons[0].Title;
                    Buttons.Visibility = Visibility.Visible;
                    MoreButton.Visibility = Visibility.Collapsed;
                    foreach (var option in block.Buttons[0].Options)
                    {
                        Buttons.Items.Add(new TextBlock() { Text = option.Text });
                    }
                    //Buttons.SelectedIndex = 0;
                    return;
                }
                else
                {
                    MoreButton.Visibility = Visibility.Visible;

                    MoreButton.Content = block.Buttons[0].Title;

                    return;

                }
            }
            else
            {
                
                if(block.Actions.Count > 0)
                {

                    if (block.Actions[0].Options.Count > 0) //android
                    {
                        ButtonsGrid.Visibility = Visibility.Visible;
                        TitleButtons.Text = block.Actions[0].Title;
                        Buttons.Visibility = Visibility.Visible;
                        MoreButton.Visibility = Visibility.Collapsed;
                        

                        foreach (var option in block.Actions[0].Options)
                        {
                            Buttons.Items.Add(new TextBlock() { Text = option.Text });
                        }
                        
                        return;
                    }
                    else
                    {
                        MoreButton.Visibility = Visibility.Visible;

                        MoreButton.Content = block.Actions[0].Title;

                        return;

                    }

                }

                return;
            }
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            try
            {
                var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

                if (block.Actions.Count > 0)
                {
                    var bnt = block.Actions[0];

                    navigationService.OpenSection(bnt.SectionId);
                    return;
                }

                var button = block.Buttons[0];

                navigationService.OpenSection(button.SectionId);
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, ex.Message);
            }


        }

        private async void ButtonsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            try
            {
                var comboBox = sender as ComboBox;

                var current = comboBox.SelectedIndex;

                OptionButton option;
                if(block.Buttons != null)
                {
                    option = block.Buttons[0].Options[current];

                }else
                {
                    option = block.Actions[0].Options[current];

                }

                var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

                navigationService.ReplaceBlocks(option.ReplacementId);
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, ex.Message);
            }


            //throw new NotImplementedException();
        }
    }
}
