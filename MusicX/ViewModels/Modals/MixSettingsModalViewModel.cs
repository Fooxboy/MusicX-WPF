using System;
using MusicX.Core.Models.Mix;
using MusicX.Core.Services;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using Wpf.Ui.Common;

namespace MusicX.ViewModels.Modals;

public class MixSettingsModalViewModel(VkService vkService) : BaseViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public bool IsLoading { get; set; }
        
    public ObservableCollection<MixSettingsCategoryViewModel> Categories { get; set; } = [];
        
    public ICommand? ApplyCommand { get; set; }
    public ICommand? ResetCommand { get; set; }

    public async Task LoadSettings(string mixId)
    {
        IsLoading = true;

        var settings = await vkService.GetStreamMixSettings(mixId);

        IsLoading = false;
        Title = settings.Settings.Title;
        Subtitle = settings.Settings.Subtitle;
        Categories = new(settings.Settings.Categories.Select(b => new MixSettingsCategoryViewModel(b)));
    }
}

public class MixSettingsCategoryViewModel : BaseViewModel
{
    public string Title { get; set; }
    public string Id { get; set; }
    public string Type { get; set; }
    public ObservableCollection<MixSettingsOptionViewModel> Options { get; set; }
    
    public MixSettingsCategoryViewModel(MixCategory category)
    {
        Title = category.Title;
        Id = category.Id;
        Type = category.Type;
        Options = new(category.Options.Select(o => new MixSettingsOptionViewModel(o)));
    }
}

public class MixSettingsOptionViewModel : BaseViewModel
{
    public string Title { get; set; }
    public string Id { get; set; }
    
    public byte[]? Icon { get; set; }
    
    public bool Selected { get; set; }
    
    public MixSettingsOptionViewModel(MixOption option)
    {
        Title = option.Title;
        Id = option.Id;
        if (!string.IsNullOrEmpty(option.IconUri))
            Load(option.IconUri).SafeFireAndForget();
    }

    private async Task Load(string uri)
    {
        using var httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        });

        await using var stream = await httpClient.GetStreamAsync(uri);

        var jsonObject = await JsonNode.ParseAsync(stream);
        
        if (jsonObject is null) return;

        var base64Image = jsonObject["assets"]![0]!["p"]!.AsValue().ToString();

        Icon = Convert.FromBase64String(base64Image["data:image/png;base64,".Length..]);
    }
}