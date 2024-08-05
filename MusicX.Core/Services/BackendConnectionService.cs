using System.Net.Http.Json;
using MusicX.Core.Models;
using MusicX.Shared;
using Newtonsoft.Json;
using NLog;

namespace MusicX.Core.Services;

public class BackendConnectionService(Logger logger, string appVersion)
{
    private string? _token;
    public readonly HttpClient Client = new();

    public string Host { get; set; } =
#if DEBUG
        "http://localhost:2024";
#else
        "https://musicx.zznty.ru";
#endif

    public async void ReportMetric(string eventName, string? source = null)
    {
        if (string.IsNullOrEmpty(_token))
            throw new NullReferenceException("Token is null");
        
        try
        {
            using var response = await Client.PostAsJsonAsync($"/metrics/{eventName}/report", new ReportMetricRequest(appVersion, source));
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            logger.Error(e, "Error while reporting event metric {0}", eventName);
            throw;
        }
    }

    public async Task<string> GetTokenAsync(long userId)
    {
        if (_token is not null) return _token;
        logger.Info("Получение временного токена Слушать вместе");

        var host = await GetHostAsync();

        Client.BaseAddress = new(host);
        using var response = await Client.PostAsJsonAsync("/token", userId);
        response.EnsureSuccessStatusCode();

        _token = await response.Content.ReadFromJsonAsync<string>() ??
                       throw new NullReferenceException("Got null response for token request");
        
        Client.DefaultRequestHeaders.Authorization = new("Bearer", _token);
        
        return _token;
    }

    private async Task<string> GetHostAsync()
    {
        if (!string.IsNullOrEmpty(Host))
            return Host;

        try
        {
            logger.Info("Получение адресса сервера Послушать вместе");
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://fooxboy.blob.core.windows.net/musicx/ListenTogetherServers.json");

                var contents = await response.Content.ReadAsStringAsync();

                var servers = JsonConvert.DeserializeObject<ListenTogetherServersModel>(contents);

                return Host =
#if DEBUG
                    servers.Test;
#else
                    servers.Production;
#endif
            }
        }
        catch(Exception)
        {
            return "http://212.192.40.71:5000";
        }
            
    }
}