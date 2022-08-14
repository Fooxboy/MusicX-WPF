using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            navigationService = StaticService.Container.Resolve<Services.NavigationService>();
            vkService = StaticService.Container.Resolve<VkService>();
            logger = StaticService.Container.Resolve<Logger>();

        }

        public Suggestion Suggestion { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Title.Text = Suggestion.Title;
                Subtitle.Text = Suggestion.Subtitle;
            }catch (Exception ex)
            {
                logger.Error("Failed load suggestion control");
                logger.Error(ex, ex.Message);

                Title.Text = "Невозможно";
                Subtitle.Text = "загрузить подсказку";
            }
           
        }


        private async void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var result = await vkService.GetAudioSearchAsync(Suggestion.Title, Suggestion.Context);

                navigationService.OpenSection(result.Catalog.DefaultSection);
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
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
