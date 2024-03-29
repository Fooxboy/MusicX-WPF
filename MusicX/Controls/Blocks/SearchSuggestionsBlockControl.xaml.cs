﻿using MusicX.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для SearchSuggestionsBlockControl.xaml
    /// </summary>
    public partial class SearchSuggestionsBlockControl : UserControl
    {
        public SearchSuggestionsBlockControl()
        {
            InitializeComponent();
            this.Loaded += SearchSuggestionsBlockControl_Loaded;
        }

        private void SearchSuggestionsBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            if (block.Layout.Name == "list") SearchSuggestionsPanel.Orientation = Orientation.Vertical;
            if (block.Suggestions != null && block.Suggestions.Count > 0)
            {
                foreach (var suggestion in block.Suggestions)
                {
                    var sug = new SuggestionControl() { Height = 40, HorizontalAlignment = HorizontalAlignment.Left, HorizontalContentAlignment = HorizontalAlignment.Left, Suggestion = suggestion, Margin = new Thickness(0, 0, 10, 0) };

                    SearchSuggestionsPanel.Children.Add(sug);
                }
            }
        }
    }
}
