using MusicX.Core.Models;
using MusicX.Shared.ListenTogether.Radio;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model.Attachments;

namespace MusicX.Core.Services
{
    public class UserRadioService
    {
        private readonly Logger _logger;

        private string _host;

        public UserRadioService(Logger logger)
        {
            _logger = logger;
        }

        public async Task<List<Station>> GetStationsList()
        {
            _logger.Info("Получение списка всех доступных станций");

            var stations = await HttpRequestAsync<List<Station>>("getStations", new Dictionary<string, string>());

            _logger.Info($"Получено {stations.Count} пользовательских станций");

            return stations;
        }

        public Task<Station> CreateStationAsync(string sessionId, string title, string cover, string decription, long ownerId, string ownerName, string ownerPhoto)
        {
            _logger.Info($"Создание пользовательской радиостанции с ID {sessionId}");

            var p = new Dictionary<string, string>()
            {
                {"sessionId", sessionId },
                {"title", title},
                {"cover", cover },
                {"decription", decription },
                {"ownerId", ownerId.ToString() },
                {"ownerName", ownerName },
                {"ownerPhoto", ownerPhoto}
            };

            return HttpRequestAsync<Station>("createStation", p);
        }

        public Task<bool> DeleteStationAsync(string sessionId)
        {
            _logger.Info($"Удаление пользовательской радиостанции с ID {sessionId}");

            var p = new Dictionary<string, string>()
            {
                {"sessionId", sessionId }
            };

            return HttpRequestAsync<bool>("deleteStation", p);
        }

        private async Task<TResponse> HttpRequestAsync<TResponse>(string method, Dictionary<string, string> parameters)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(await GetHostNameAsync());
                    var p = parameters.Select(x => x.Key + "=" + x.Value);

                    var result = await httpClient.GetAsync("/radio/" + method + "?" + string.Join("&", p));

                    result.EnsureSuccessStatusCode();

                    var resultJson = await result.Content.ReadAsStringAsync();

                    return System.Text.Json.JsonSerializer.Deserialize<TResponse>(resultJson);
                }
            }catch(Exception ex)
            {
                _logger.Info($"Произошла ошибка при запросе: {ex}");
                _logger.Error(ex);
                throw;
            }
           
        }

        private async Task<string> GetHostNameAsync()
        {
            try
            {
                if(!string.IsNullOrEmpty(_host))
                {
                    return _host;
                }

                _logger.Info("Получение адресса сервера Послушать вместе");
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://fooxboy.blob.core.windows.net/musicx/ListenTogetherServers.json");

                    var contents = await response.Content.ReadAsStringAsync();

                    var servers = JsonConvert.DeserializeObject<ListenTogetherServersModel>(contents);

                    return _host =
#if DEBUG
                    servers.Test;
#else
                    servers.Production;
#endif
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return "127.0.0.1:2023";
            }
        }
    }
}
