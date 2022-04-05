using DryIoc;
using MusicX.Core.Models;
using MusicX.Services;
using MusicX.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для SectionView.xaml
    /// </summary>
    public partial class SectionView : Page
    {
        public SectionViewModel ViewModel { get; set; }
        private readonly Logger logger;

        public SectionView()
        {
            InitializeComponent();

            ViewModel = StaticService.Container.Resolve<SectionViewModel>();

            DataContext = ViewModel;

            ViewModel.ContentLoaded += ViewModel_ContentLoaded;


            logger = StaticService.Container.Resolve<Logger>();

        }

        private void ViewModel_ContentLoaded()
        {
            var amim = (Storyboard)(this.Resources["LoadedAmination"]);
            amim.Begin();

            ContentGrid.Margin = new Thickness(0, 0, 0, 0);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
          
        }

        public async Task LoadSection(string sectionId, bool showTitle = false)
        {
            try
            {
                if (BlocksScrollView.Items.Count > 0)
                {
                    BlocksScrollView.ScrollIntoView(BlocksScrollView.Items[0]);
                }


                var amim = (Storyboard)(this.Resources["LoaderAmimation"]);
                amim.Begin();


                await ViewModel.LoadSection(sectionId, showTitle);
            }catch (Exception ex)
            {
                logger.Error("FATAL ERROR IN LOAD SECTION");
                logger.Error(ex, ex.Message);
            }
            

        }

        public async Task ReplaceBlocks(string replaceId)
        {
            try
            {
                await this.ViewModel.ReplaceBlocks(replaceId);

            }catch(Exception ex)
            {
                logger.Error("FATAL ERROR IN REPLACE BLOCK");
                logger.Error(ex, ex.Message);
            }
        }

        public async Task SetBlocks(List<Block> blocks, string next)
        {
            try
            {
                if (BlocksScrollView.Items.Count > 0)
                {
                    BlocksScrollView.ScrollIntoView(BlocksScrollView.Items[0]);
                }


                await ViewModel.LoadBlocks(blocks, next);
            }catch(Exception ex)
            {
                logger.Error("FATAL ERROR IN SET BLOCK");
                logger.Error(ex, ex.Message);
            }
            
        }

        private async void BlocksScrollView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                if (e.ExtentHeight < 400) return;

                //Debug.WriteLine($" ExtentHeight: {e.ExtentHeight} | VerticalOffset: {e.VerticalOffset + Window.GetWindow(this).ActualHeight }");
                if (e.ExtentHeight < e.VerticalOffset + Window.GetWindow(this).ActualHeight)
                {
                    await ViewModel.LoadMore();
                }
            }catch(Exception ex)
            {
                logger.Error("FATAL ERROR IN SCROLL VIEW CHANGED");
                logger.Error(ex, ex.Message);
            }
            
        }

        public async Task LoadArtistSection(string artistId)
        {
            try
            {
                if (BlocksScrollView.Items.Count > 0)
                {
                    BlocksScrollView.ScrollIntoView(BlocksScrollView.Items[0]);
                }

                var amim = (Storyboard)(this.Resources["LoaderAmimation"]);
                amim.Begin();
                await ViewModel.LoadArtistSection(artistId);
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async Task LoadSearchSection(string query)
        {
            try
            {
                if (BlocksScrollView.Items.Count > 0)
                {
                    BlocksScrollView.ScrollIntoView(BlocksScrollView.Items[0]);
                }

                var amim = (Storyboard)(this.Resources["LoaderAmimation"]);
                amim.Begin();
                await ViewModel.LoadSearchSection(query);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }
    }
}
