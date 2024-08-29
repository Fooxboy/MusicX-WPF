using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AsyncAwaitBestPractices;
using MusicX.Controls;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using NLog;
using VkNet.Exception;
using Wpf.Ui;

namespace MusicX.ViewModels
{
    public enum ContentState
    {
        Loading,
        Loaded
    }
    
    public class SectionViewModel : BaseViewModel, INotifyOnActivated, IDisposable
    {
        private readonly VkService vkService;
        private readonly Logger logger;
        private readonly ISnackbarService _snackbarService;
        private readonly ConfigService configService;
        private readonly SectionEventService _eventService;

        public ContentState ContentState { get; set; }
        public bool IsLoadingMore { get; set; }
        public SectionType SectionType { get; set; }
        public string SectionId { get; set; }
        public Section Section { get; set; }
        public string? Next { get; set; }
        
        public Artist? Artist { get; set; }

        public ObservableRangeCollection<BlockViewModel> Blocks { get; } = [];

        public SectionViewModel(VkService vkService, Logger logger, ISnackbarService snackbarService,
            ConfigService configService, SectionEventService eventService)
        {
            this.vkService = vkService;
            this.logger = logger;
            _snackbarService = snackbarService;
            this.configService = configService;
            _eventService = eventService;
            
            _eventService.Event += EventServiceOnEvent;
        }

        private async void EventServiceOnEvent(object? sender, string e)
        {
            var changed = false;
            for (var i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                
                if (!block.ListenEvents.Contains(e))
                    continue;
                
                changed = true;

                // не работает хз
                // var response = await vkService.GetBlockItems(block.Id);
                //
                // Blocks[i] = new BlockViewModel(response.Block);
            }
            
            if (changed)
                await LoadAsync();
        }

        public async Task LoadAsync()
        {
            ContentState = ContentState.Loading;
            try
            {
                await (SectionType switch
                {
                    SectionType.None or SectionType.SearchResult => LoadSection(SectionId),
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
                    if (section.Section.Blocks is [var singleBlock] && Blocks[^1].DataType == singleBlock.DataType &&
                        Blocks[^1].Layout.Name == singleBlock.Layout.Name)
                    {
                        Blocks[^1].MergeBlock(singleBlock);
                        return;
                    }
                    
                    foreach (var block in section.Section.Blocks)
                    {
                        Blocks.Add(new(block));
                    }
                });

                Next = section.Section.NextFrom;
                IsLoadingMore = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load more for section");

                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог подргрузить контент");
            }
        }

        public async Task ReplaceBlocks(string replaceId)
        {
            nowOpenSearchSug = false;
            try
            {
                logger.Info("Replace blocks...");
                var replaces = await vkService.ReplaceBlockAsync(replaceId).ConfigureAwait(false);

                var toReplaceBlockIds = replaces.Replacements.ReplacementsModels.SelectMany(b => b.FromBlockIds)
                    .ToHashSet();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Blocks.RemoveRange(Blocks.Where(b => toReplaceBlockIds.Contains(b.Id)).ToArray());
                    Blocks.AddRangeSequential(replaces.Replacements.ReplacementsModels.SelectMany(b => b.ToBlocks)
                        .Select(b => new BlockViewModel(b)));
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to replace blocks");
                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог заменить блоки");
            }
        }

        public async Task LoadBlocks(List<Block> blocks, string nextValue)
        {
            Next = nextValue;
            nowOpenSearchSug = false;

            var config = await configService.GetConfig();

            if (config.NotifyMessages is null)
                config.NotifyMessages = new() { ShowListenTogetherModal = true, LastShowedTelegramBlock = null };

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
                        Blocks.ReplaceRange(blocks.Select(b => new BlockViewModel(b)));
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, ex.Message);
                        _snackbarService.ShowException("Произошла ошибка", "MusicX не смог подргрузить контент");
                    }
                });

                logger.Info($"Loaded {blocks.Count} blocks");
                ContentState = ContentState.Loaded;
            }
            catch (Exception ex)
            {
                ContentState = ContentState.Loaded;
                logger.Error(ex, "Failed to load section from blocks");

                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог загрузить контент");
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
                    Blocks.Add(new(new() {DataType = "none", Layout = new() {Name = "header", Title = "Ничего не найдено"}}));
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to load section by id");

                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог загрузить контент");
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
                logger.Error(ex, "Failed to load artist section {ArtistId}", artistId);

                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог загрузить контент");
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
                logger.Error(ex, "Failed to load search section {Query}", query);

                _snackbarService.ShowException("Произошла ошибка", "MusicX не смог загрузить контент");

            }
        }
        public void OnActivated()
        {
            if (Section is null)
                LoadAsync().SafeFireAndForget();
            else
                _eventService.Event += EventServiceOnEvent;
        }

        public void Dispose()
        {
            _eventService.Event -= EventServiceOnEvent;
        }
    }
}
