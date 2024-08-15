using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Media;
using Microsoft.AppCenter.Crashes;
using Wpf.Ui.Appearance;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для MusicCategoryBlockControl.xaml
    /// </summary>
    public partial class MusicCategoryBlockControl : UserControl
    {

        private readonly VkService vkService;
        private readonly Services.NavigationService navigationService;

        public static readonly DependencyProperty AppThemeProperty =
            DependencyProperty.Register(nameof(AppTheme), typeof(ApplicationTheme), typeof(MusicCategoryBlockControl));

        public ApplicationTheme AppTheme
        {
            get => (ApplicationTheme)GetValue(AppThemeProperty);
            set => SetValue(AppThemeProperty, value);
        }

        public MusicCategoryBlockControl()
        {
            InitializeComponent();
            Unloaded += OnUnloaded;

            AppTheme = ApplicationThemeManager.GetAppTheme();
            ApplicationThemeManager.Changed += ApplicationThemeOnChanged;

            vkService = StaticService.Container.GetRequiredService<VkService>();
            navigationService = StaticService.Container.GetRequiredService<Services.NavigationService>();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ApplicationThemeManager.Changed -= ApplicationThemeOnChanged;
        }

        private void ApplicationThemeOnChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
        {
            AppTheme = currentApplicationTheme;
        }
        
        public List<Link> Links => (DataContext as Block)?.Links ?? new();

        private async Task OpenPage(Link link)
        {
            try
            {
                if (link.Meta?.ContentType is "custom")
                {
                    navigationService.OpenSection(link.Meta.TrackCode);
                    return;
                }
                
                var music = await vkService.GetAudioCatalogAsync(link.Url);
                navigationService.OpenSection(music.Catalog.DefaultSection);

                return;
            }
            catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
            }

        }

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Control { DataContext: Link link })
                await OpenPage(link);
        }
    }
}
