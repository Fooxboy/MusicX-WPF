using Microsoft.AppCenter.Analytics;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Services;
using MusicX.Services.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для LyricsControl.xaml
    /// </summary>
    public partial class LyricsControl : UserControl
    {
        private List<LyricsTimestamp> _allLines;

        private List<string> _allText;

        private bool _autoScroll;

        public LyricsControl()
        {
            _autoScroll = true;
            InitializeComponent();
        }

        public void SetLines(List<LyricsTimestamp> lines)
        {
            var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
            Analytics.TrackEvent("OpenAudioText", properties);

            this._autoScroll = true;
            this._allText = null;
            this._allLines = lines;

            ListLines.Children.Clear();
            foreach (var line in _allLines)
            {
                var textControl = new TextBlock() { Text = line.Line, FontSize = 25, Opacity = 0.5, Margin = new System.Windows.Thickness(0, 5, 0, 5), Cursor = Cursors.Hand};
                textControl.MouseEnter += TextControl_MouseEnter;
                textControl.MouseLeave += TextControl_MouseLeave;
                textControl.MouseLeftButtonDown += TextControl_MouseLeftButtonDown;
                ListLines.Children.Add(textControl);
            }
        }

        public void SetLines(List<string> lines)
        {
            var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
            Analytics.TrackEvent("OpenAudioText", properties);

            this._allLines = null;
            SetAutoScrollMode(false);
            this._allText = lines;

            ListLines.Children.Clear();
            foreach (var line in _allText)
            {
                var textControl = new TextBlock() { Text = line, FontSize = 25, Opacity = 0.5, Margin = new System.Windows.Thickness(0, 5, 0, 5) };
                ListLines.Children.Add(textControl);
            }
        }

        private void TextControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_autoScroll) return;

            (sender as TextBlock).Opacity = 0.5;
            e.Handled = true;
        }

        private void TextControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textBlocks = ListLines.Children.Cast<TextBlock>().ToList();
            var position = textBlocks.IndexOf(sender as TextBlock);

            var playerService = StaticService.Container.GetRequiredService<PlayerService>();

            var item = _allLines[position];

            playerService.Seek(TimeSpan.FromMilliseconds(item.Begin));
            SetAutoScrollMode(true);
        }

        private void TextControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_autoScroll) return;

            (sender as TextBlock).Opacity = 0.8;
            e.Handled = true;
        }

        public void SetAutoScrollMode(bool autoScrollMode)
        {
            if(autoScrollMode && _allText != null)
            {
                _autoScroll = false;
            }

            _autoScroll = autoScrollMode;

            if(!_autoScroll)
            {
                var textBlocks = ListLines.Children.Cast<TextBlock>().ToList();

                var whiteLines = textBlocks.Where(x => x.Opacity == 1);

                foreach (var whiteLine in whiteLines)
                {
                    whiteLine.Opacity = 0.5;
                }
            }
        }

        public void ScrollToTime(int time)
        {
            if (!_autoScroll) return;


            var element = _allLines.FirstOrDefault(t => t.Begin <= time && t.End >= time);

            if (element is null) return;

            var position = _allLines.IndexOf(element);

            var scrollOffset = position * 43;

            CurrentScrollViewer.ScrollToVerticalOffsetWithAnimation(scrollOffset);

            var textBlocks = ListLines.Children.Cast<TextBlock>().ToList();

            var whiteLines = textBlocks.Where(x => x.Opacity == 1);

            foreach (var whiteLine in whiteLines)
            {
                whiteLine.Opacity = 0.5;
            }

            textBlocks[position].Opacity = 1;
        }

        private void CurrentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }
    }
}
