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
    /// Логика взаимодействия для SearchSuggestionsBlockControl.xaml
    /// </summary>
    public partial class SearchSuggestionsBlockControl : UserControl
    {
        public Block Block { get; set; }
        public SearchSuggestionsBlockControl()
        {
            InitializeComponent();
            this.Loaded += SearchSuggestionsBlockControl_Loaded;
        }

        private void SearchSuggestionsBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Block.Suggestions != null && Block.Suggestions.Count > 0)
            {
                foreach (var suggestion in Block.Suggestions)
                {

                    SearchSuggestionsPanel.Children.Add(new SuggestionControl() { Height = 40, Suggestion = suggestion, Margin = new Thickness(0,0,10,0) });
                }
            }
        }
    }
}
