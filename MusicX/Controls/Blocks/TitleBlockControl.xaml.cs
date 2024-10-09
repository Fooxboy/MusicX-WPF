using MusicX.Services;
using NLog;
using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MusicX.ViewModels;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для TitleBlockControl.xaml
    /// </summary>
    public partial class TitleBlockControl : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(BlockViewModel), typeof(TitleBlockControl), new PropertyMetadata(PropertyChangedCallback));

        public BlockViewModel ViewModel
        {
            get => (BlockViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        
        public TitleBlockControl()
        {
            InitializeComponent();
        }
        
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TitleBlockControl control && e.NewValue is not null)
                control.Fill();
        }

        private void Fill()
        {
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

            if (ViewModel.Layout.Name == "header_compact")
            {
                Title.Opacity = 0.5;
                Title.FontSize = 15;
            }

            Title.Content = ViewModel.Layout.Title;

            if(ViewModel.Layout.TopTitle is not null || ViewModel.Layout.Subtitle is not null)
            {
                Subtitle.Text = ViewModel.Layout.TopTitle?.Text ?? ViewModel.Layout.Subtitle;
                Subtitle.Visibility = Visibility.Visible;
                if (ViewModel.Buttons.Count == 1)
                    Subtitle.Margin = new(11, 0, 11, 0);
            }

            if (ViewModel.Badge != null)
            {
                BadgeHeader.Text = ViewModel.Badge.Text;
                BadgeHeader.Visibility = Visibility.Visible;
            }

            if (ViewModel.Buttons is { Count: > 0 }) //ios
            {
                if (ViewModel.Buttons[0].Options.Count > 0)
                {
                    ButtonsGrid.Visibility = Visibility.Visible;
                    TitleButtons.Text = ViewModel.Buttons[0].Title;
                    Buttons.Visibility = Visibility.Visible;
                    foreach (var option in ViewModel.Buttons[0].Options)
                    {
                        Buttons.Items.Add(new TextBlock() { Text = option.Text });
                    }
                    //Buttons.SelectedIndex = 0;
                    return;
                }
            }
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

                var button = ViewModel.Buttons[0];

                if (!string.IsNullOrEmpty(button.SectionId))
                    navigationService.OpenSection(button.SectionId);
            }
            catch (Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, "Failed to open extended section in title block");
            }


        }

        private async void ButtonsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboBox = sender as ComboBox;

                var current = comboBox.SelectedIndex;

                var option = ViewModel.Buttons[0].Options[current];

                var navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();

                navigationService.ReplaceBlocks(option.ReplacementId);
            }
            catch (Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, "Failed to replace blocks in title combobox");
            }


            //throw new NotImplementedException();
        }
    }
}
