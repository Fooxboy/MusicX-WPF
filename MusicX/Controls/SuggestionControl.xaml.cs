﻿using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для SuggestionControl.xaml
    /// </summary>
    public partial class SuggestionControl : UserControl
    {
        private readonly Services.NavigationService navigationService;
        private readonly Logger logger;
        private readonly VkService vkService;


        public SuggestionControl()
        {
            InitializeComponent();
            navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
            vkService = StaticService.Container.GetRequiredService<VkService>();
            logger = StaticService.Container.GetRequiredService<Logger>();

        }

        public static readonly DependencyProperty SuggestionProperty = DependencyProperty.Register(
            nameof(Suggestion), typeof(Suggestion), typeof(SuggestionControl), new PropertyMetadata(default(Suggestion)));

        public Suggestion Suggestion
        {
            get => (Suggestion)GetValue(SuggestionProperty);
            set => SetValue(SuggestionProperty, value);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Title.Text = Suggestion.Title;
                Subtitle.Text = Suggestion.Subtitle;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed load suggestion control");

                Title.Text = "Невозможно";
                Subtitle.Text = "загрузить подсказку";
            }
           
        }


        private async void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var result = await vkService.GetAudioSearchAsync(Suggestion.Title, Suggestion.Context);

                navigationService.OpenSection(result.Catalog.DefaultSection, SectionType.SearchResult);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Failed to load section from suggestion control");
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            var amim = (Storyboard)(this.Resources["OpenAnimation"]);
            amim.Begin();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            var amim = (Storyboard)(this.Resources["CloseAnimation"]);
            amim.Begin();
        }
    }
}
