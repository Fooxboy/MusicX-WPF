using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MusicX.ViewModels
{
    public class SectionViewModel : BaseViewModel
    {
        private readonly VkService vkService;
        private readonly NavigationService navigationService;
        private readonly Logger logger;
        private readonly NotificationsService notificationsService;

        public event Action ContentLoaded;

        public Visibility VisibleLoading { get; set; }

        public Visibility VisibleContent { get; set; }

        public Section Section { get; set; }
        public string Next { get; set; }

        public ObservableCollection<Block> Blocks { get; set; } = new ObservableCollection<Block>();

        public SectionViewModel(VkService vkService, NavigationService navigationService, Logger logger, NotificationsService notificationsService)
        {
            this.vkService = vkService;
            VisibleContent = Visibility.Collapsed;
            VisibleLoading = Visibility.Collapsed;
            this.navigationService = navigationService;
            this.logger = logger;
            this.notificationsService = notificationsService;
        }

        private bool nowLoading = false;


        public async Task LoadMore()
        {
            int indexLoad = -1;

            await Task.Run(async () =>
            {
                try
                {

                    if (nowLoading) return;
                    if (Next == null) return;

                    logger.Info($"Load more for {Next} next");

                    nowLoading = true;

                    await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        var loadBlock = new Block() { DataType = "loader" };

                        Blocks.Add(loadBlock);
                        indexLoad = Blocks.IndexOf(loadBlock);

                        Changed("Blocks");

                    }));
                    logger.Info($"Load more section content with next id = {Next}");

                    var section = await vkService.GetSectionAsync(Section.Id, Next).ConfigureAwait(false);

                    foreach (var block in section.Section.Blocks)
                    {
                        await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            this.Blocks.Add(block);
                        }));
                    }

                    Next = section.Section.NextFrom;

                    Changed("Blocks");
                    nowLoading = false;
                    GC.Collect();

                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);

                    notificationsService.Show("Произошла ошибка", "MusicX не смог подргрузить контент");
                }
            });

            if(indexLoad != -1)
            {
                Blocks.RemoveAt(indexLoad);

                Changed("Blocks");
            }
        }

        public async Task ReplaceBlocks(string replaceId)
        {
            nowOpenSearchSug = false;
            try
            {
                logger.Info("Replace blocks...");
                var replaces = await vkService.ReplaceBlockAsync(replaceId).ConfigureAwait(false);

                foreach (var replace in replaces.Replacements.ReplacementsModels)
                {
                    var startIndex = Blocks.IndexOf(Blocks.SingleOrDefault(b => b.Id == replace.FromBlockIds.First()));

                    if (startIndex == -1)
                    {
                        break;
                    }

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        for (int i = startIndex; i < Blocks.Count - 1; i++)
                        {
                            Blocks.RemoveAt(i);
                        }

                        foreach (var block in replace.ToBlocks)
                        {
                            Blocks.Add(block);
                        }
                    });



                }


            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                    notificationsService.Show("Произошла ошибка", "MusicX не смог заменить блоки");

            }
            Changed("Blocks");
        }

        public async Task LoadBlocks(List<Block> blocks, string nextValue)
        {
            this.Next = nextValue;
            nowOpenSearchSug = false;
            navigationService.AddHistory(Models.Enums.NavigationSource.Section, (blocks, this.Next));

            await Task.Run(async () =>
            {
                try
                {
                    logger.Info("Load from blocks");
                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            this.Blocks.Clear();


                            foreach (var block in blocks)
                            {
                                this.Blocks.Add(block);
                            }
                            Changed("Blocks");
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, ex.Message);

                            notificationsService.Show("Произошла ошибка", "MusicX не смог подргрузить контент");
                        }

                    });

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        VisibleLoading = Visibility.Collapsed;
                        VisibleContent = Visibility.Visible;

                        Changed("VisibleLoading");
                        Changed("VisibleContent");
                        Changed("Blocks");

                        this.ContentLoaded?.Invoke();

                    });

                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);

                    notificationsService.Show("Произошла ошибка", "MusicX не смог загрузить контент");

                }
            });
            
        }

        public async Task LoadSection(string sectionId, bool showTitle = false)
        {
            nowOpenSearchSug = false;

            try
            {
                logger.Info($"Load section {sectionId}");

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    VisibleLoading = Visibility.Visible;
                    VisibleContent = Visibility.Collapsed;

                    Changed("VisibleLoading");
                    Changed("VisibleContent");
                });


                var section = await vkService.GetSectionAsync(sectionId).ConfigureAwait(false);

                this.Section = section.Section;

                navigationService.AddHistory(Models.Enums.NavigationSource.Section, (section.Section.Blocks, section.Section.NextFrom));

                this.Section = section.Section;
                logger.Info($"Loaded {section.Section.Blocks.Count} blocks");


                var obsCollection = new ObservableCollection<Block>();

                if (showTitle)
                {
                    obsCollection.Add(new Block() { DataType = "none", Layout = new Layout() { Name = "header", Title = Section.Title } });
                }
                foreach (var block in section.Section.Blocks)
                {
                    obsCollection.Add(block);
                }

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        this.Blocks = null;

                        this.Blocks = obsCollection;

                        obsCollection = null;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Fatal error during show blocks:");

                        logger.Error(ex, ex.Message);
                    }

                });

                Next = section.Section.NextFrom;


                await Application.Current.Dispatcher.BeginInvoke(() =>
                {

                    VisibleLoading = Visibility.Collapsed;
                    VisibleContent = Visibility.Visible;

                    Changed("VisibleLoading");
                    Changed("VisibleContent");
                    Changed("Blocks");

                    this.ContentLoaded?.Invoke();

                });


            }
            catch (Exception ex)
            {
                logger.Error("Fatal error in Section View Model:");

                logger.Error(ex, ex.Message);

                notificationsService.Show("Произошла ошибка", "MusicX не смог загрузить контент");

            }

        }

        public async Task LoadArtistSection(string artistId)
        {
            nowOpenSearchSug = false;
            try
            {
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    VisibleLoading = Visibility.Visible;
                    VisibleContent = Visibility.Collapsed;

                    Changed("VisibleLoading");
                    Changed("VisibleContent");
                });

                var artist = await vkService.GetAudioArtistAsync(artistId);


                await this.LoadSection(artist.Catalog.DefaultSection);
                
            }
            catch (Exception ex)
            {
                logger.Error($"Fatal error in Load artist section with artistId = {artistId}");

                logger.Error(ex, ex.Message);

                notificationsService.Show("Произошла ошибка", "MusicX не смог загрузить контент");

            }
        }

        private bool nowOpenSearchSug = false;
        public async Task LoadSearchSection(string query)
        {
            try
            {
                if (query == null && nowOpenSearchSug) return;
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    VisibleLoading = Visibility.Visible;
                    VisibleContent = Visibility.Collapsed;

                    Changed("VisibleLoading");
                    Changed("VisibleContent");
                });
                var res = await vkService.GetAudioSearchAsync(query);

                if(query == null)
                {
                   
                    try
                    {
                        res.Catalog.Sections[0].Blocks[1].Suggestions = res.Suggestions;
                        await this.LoadBlocks(res.Catalog.Sections[0].Blocks, null);
                        nowOpenSearchSug = true;
                    }
                    catch (Exception ex)
                    {
                        await this.LoadBlocks(res.Catalog.Sections[0].Blocks, null);
                    }

                    return;
                }

                nowOpenSearchSug = false;
                await this.LoadSection(res.Catalog.DefaultSection);

            }
            catch (Exception ex)
            {
                logger.Error($"Fatal error in load search section with query = {query}");

                logger.Error(ex, ex.Message);

                notificationsService.Show("Произошла ошибка", "MusicX не смог загрузить контент");

            }
        }
    }
}
