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

        public Visibility VisibleLoading { get; set; }

        public Visibility VisibleContent { get; set; }

        public Section Section { get; set; }
        public string Next { get; set; }

        public ObservableCollection<Block> Blocks { get; set; } = new ObservableCollection<Block>();

        public SectionViewModel(VkService vkService, NavigationService navigationService, Logger logger)
        {
            this.vkService = vkService;
            VisibleContent = Visibility.Collapsed;
            VisibleLoading = Visibility.Collapsed;
            this.navigationService = navigationService;
            this.logger = logger;
        }

        private bool nowLoading = false;


        public async Task LoadMore()
        {
            await Task.Run(async () =>
            {
                try
                {

                    if (nowLoading) return;
                    if (Next == null) return;
                    nowLoading = true;
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

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var msgbox = new WPFUI.Controls.MessageBox();
                        msgbox.Foreground = Brushes.Black;
                        msgbox.Show("Exception", ex.Message);

                    });
                }
            });
            
        }

        public async Task ReplaceBlocks(string replaceId)
        {
            try
            {
                logger.Info("Replace blocks...");
                var replaces = await vkService.ReplaceBlockAsync(replaceId).ConfigureAwait(false);

                foreach (var replace in replaces.Replacements.ReplacementsModels)
                {
                    var startIndex = Blocks.IndexOf(Blocks.SingleOrDefault(b => b.Id == replace.FromBlockId));

                    if (startIndex == -1)
                    {
                        foreach (var block in replace.ToBlocks)
                        {

                            await Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                foreach (var block in replace.ToBlocks)
                                {
                                   // if (block.DataType == "none" && block.Buttons[0].Options.Count > 1) continue;
                                    Blocks.Add(block);
                                }
                            });
                        }

                        break;
                    }

                    startIndex += 1;

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        for (int i = startIndex; i < Blocks.Count - 1; i++)
                        {
                            Blocks.RemoveAt(i);

                        }

                        foreach (var block in replace.ToBlocks)
                        {
                            //if (block.DataType == "none" && block.Buttons[0].Options.Count > 1) continue;
                            Blocks.Add(block);
                        }
                    });



                }

                GC.Collect();

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msgbox = new WPFUI.Controls.MessageBox();
                    msgbox.Foreground = Brushes.Black;
                    msgbox.Show("Exception", ex.Message);

                });
            }
            Changed("Blocks");
        }

        public async Task LoadBlocks(List<Block> blocks)
        {
            navigationService.AddHistory(Models.Enums.NavigationSource.Section, blocks);

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

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var msgbox = new WPFUI.Controls.MessageBox();
                                msgbox.Foreground = Brushes.Black;
                                msgbox.Show("Exception", ex.Message);

                            });
                        }

                    });
                    GC.Collect();

                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                }
            });
            
        }

        public async Task LoadSection(string sectionId)
        {
            await Task.Run(async () =>
            {
                try
                {
                    logger.Info($"Load section {sectionId}");
                    this.Section = Section;

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        VisibleLoading = Visibility.Visible;
                        VisibleContent = Visibility.Collapsed;

                        Changed("VisibleLoading");
                        Changed("VisibleContent");
                    });


                    var section = await vkService.GetSectionAsync(sectionId).ConfigureAwait(false);

                    navigationService.AddHistory(Models.Enums.NavigationSource.Section, section.Section.Blocks);

                    this.Section = section.Section;
                    logger.Info($"Loaded {section.Section.Blocks.Count} blocks");


                    var obsCollection = new ObservableCollection<Block>();
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

                    });


                    GC.Collect();
                }
                catch (Exception ex)
                {
                    logger.Error("Fatal error in Section View Model:");

                    logger.Error(ex, ex.Message);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var msgbox = new WPFUI.Controls.MessageBox();
                        msgbox.Foreground = Brushes.Black;
                        msgbox.Show("Exception", ex.Message);

                    });
                }
            });
           
        }

        public async Task LoadArtistSection(string artistId)
        {
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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msgbox = new WPFUI.Controls.MessageBox();
                    msgbox.Foreground = Brushes.Black;
                    msgbox.Show("Exception", ex.Message);

                });
            }
        }

        public async Task LoadSearchSection(string query)
        {
            try
            {
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    VisibleLoading = Visibility.Visible;
                    VisibleContent = Visibility.Collapsed;

                    Changed("VisibleLoading");
                    Changed("VisibleContent");
                });
                var res = await vkService.GetAudioSearchAsync(query);

                await this.LoadSection(res.Catalog.DefaultSection);

            }
            catch (Exception ex)
            {
                logger.Error($"Fatal error in load search section with query = {query}");

                logger.Error(ex, ex.Message);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msgbox = new WPFUI.Controls.MessageBox();
                    msgbox.Foreground = Brushes.Black;
                    msgbox.Show("Exception", ex.Message);

                });
            }
        }
    }
}
