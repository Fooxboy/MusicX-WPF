using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AsyncAwaitBestPractices;
using MusicX.Controls;
using MusicX.Helpers;
using VkNet.Exception;
using Microsoft.AppCenter.Crashes;

namespace MusicX.ViewModels
{
    public enum ContentState
    {
        Loading,
        Loaded
    }
    
    public class SectionViewModel : BaseViewModel, INotifyOnActivated
    {
        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly NotificationsService notificationsService;
        private readonly ConfigService configService;

        public ContentState ContentState { get; set; }
        public bool IsLoadingMore { get; set; }
        public SectionType SectionType { get; set; }
        public string SectionId { get; set; }
        public Section Section { get; set; }
        public string? Next { get; set; }
        
        public Artist? Artist { get; set; }

        public ObservableRangeCollection<Block> Blocks { get; set; } = new();

        public SectionViewModel(VkService vkService, Logger logger, NotificationsService notificationsService, ConfigService configService)
        {
            this.vkService = vkService;
            this.logger = logger;
            this.notificationsService = notificationsService;
            this.configService = configService;
        }

        public async Task LoadAsync()
        {
            ContentState = ContentState.Loading;
            try
            {
                await (SectionType switch
                {
                    SectionType.None => LoadSection(SectionId),
                    SectionType.Artist => LoadArtistSection(SectionId),
                    SectionType.Search => LoadSearchSection(SectionId),
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            finally
            {
                ContentState = ContentState.Loaded;
            }
        }

        public async Task LoadMore()
        {
            try
            {
                if (IsLoadingMore) return;
                if (Next == null) return;
                logger.Info($"Load more for {Next} next");

                IsLoadingMore = true;
                
                logger.Info($"Load more section content with next id = {Next}");

                var section = await vkService.GetSectionAsync(Section.Id, Next).ConfigureAwait(false);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var block in section.Section.Blocks)
                    {
                        Blocks.Add(block);
                    }
                });

                Next = section.Section.NextFrom;
                IsLoadingMore = false;
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);

                notificationsService.Show("Произошла ошибка", "MusicX не смог подргрузить контент");
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

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Blocks.RemoveRange(Blocks.ToArray()[startIndex..]);
                        Blocks.AddRange(replace.ToBlocks, NotifyCollectionChangedAction.Reset);
                    });
                }


            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
                logger.Error(ex, ex.Message);
                notificationsService.Show("Произошла ошибка", "MusicX не смог заменить блоки");
            }
        }

        public async Task LoadBlocks(List<Block> blocks, string nextValue)
        {
            Next = nextValue;
            nowOpenSearchSug = false;

            var config = await configService.GetConfig();

            if (config.NotifyMessages is null) config.NotifyMessages = new Models.NotifyMessagesConfig() { ShowListenTogetherModal = true, LastShowedTelegramBlock = null };

            if(config.NotifyMessages.LastShowedTelegramBlock is null || config.NotifyMessages.LastShowedTelegramBlock - DateTime.Now > TimeSpan.FromDays(2))
            {
                var telegramBlock = new Block() { DataType = "telegram" };

                blocks.Insert(2, telegramBlock);

                config.NotifyMessages.LastShowedTelegramBlock = DateTime.Now;

                await configService.SetConfig(config);
            }

            try
            {
                logger.Info("Load from blocks");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        Blocks.ReplaceRange(blocks);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, ex.Message);
                        notificationsService.Show("Произошла ошибка", "MusicX не смог подргрузить контент");
                    }
                });

                logger.Info($"Loaded {blocks.Count} blocks");
                ContentState = ContentState.Loaded;
            }
            catch (Exception ex)
            {
                ContentState = ContentState.Loaded;
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
                logger.Error(ex, ex.Message);

                notificationsService.Show("Произошла ошибка", "MusicX не смог загрузить контент");
            }
        }

        private async Task LoadSection(string sectionId, bool showTitle = false)
        {
            try
            {
                logger.Info($"Load section {sectionId}");

                var section = await vkService.GetSectionAsync(sectionId).ConfigureAwait(false);

                Section = section.Section;
                Artist = section.Artists?.FirstOrDefault();
                
                if (showTitle)
                    section.Section.Blocks.Insert(0, new() {DataType = "none", Layout = new() {Name = "header", Title = Section.Title}});

                await LoadBlocks(section.Section.Blocks, section.Section.NextFrom);
            }
            catch (VkApiMethodInvokeException ex) when (ex.ErrorCode == 104)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Blocks.Clear();
                    Blocks.Add(new() {DataType = "none", Layout = new() {Name = "header", Title = "Ничего не найдено"}});
                });
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("Fatal error in Section View Model:");

                logger.Error(ex, ex.Message);

                notificationsService.Show("Произошла ошибка", "MusicX не смог загрузить контент");
            }
        }

        private async Task LoadArtistSection(string artistId)
        {
            try
            {
                var artist = await vkService.GetAudioArtistAsync(artistId);
                await LoadSection(artist.Catalog.DefaultSection);
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
                logger.Error($"Fatal error in Load artist section with artistId = {artistId}");
                logger.Error(ex, ex.Message);

                notificationsService.Show("Произошла ошибка", "MusicX не смог загрузить контент");
            }
        }

        private bool nowOpenSearchSug = false;
        private async Task LoadSearchSection(string query)
        {
            try
            {
                if (query == null && nowOpenSearchSug) return;

                var res = await vkService.GetAudioSearchAsync(query);

                if(query == null)
                {
                   
                    try
                    {
                        res.Catalog.Sections[0].Blocks[1].Suggestions = res.Suggestions;
                        await LoadBlocks(res.Catalog.Sections[0].Blocks, null);
                        nowOpenSearchSug = true;
                    }
                    catch (Exception ex)
                    {
                        await LoadBlocks(res.Catalog.Sections[0].Blocks, null);
                    }

                    return;
                }

                nowOpenSearchSug = false;
                await LoadSection(res.Catalog.DefaultSection);

            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
                logger.Error($"Fatal error in load search section with query = {query}");

                logger.Error(ex, ex.Message);

                notificationsService.Show("Произошла ошибка", "MusicX не смог загрузить контент");

            }
        }
        public void OnActivated()
        {
            if (Section is null)
                LoadAsync().SafeFireAndForget();
        }
    }
}
