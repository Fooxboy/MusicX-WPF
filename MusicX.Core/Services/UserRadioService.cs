using MusicX.Shared.ListenTogether.Radio;
using NLog;
using System.Net.Http.Json;

namespace MusicX.Core.Services
{
    public class UserRadioService(Logger logger, BackendConnectionService backendConnectionService)
    {
        public bool IsStarted { get; private set; }

        public async Task<List<Station>> GetStationsList()
        {
            logger.Info("Получение списка всех доступных станций");

            var stations = await HttpRequestAsync<List<Station>>("getStations", new Dictionary<string, string>());

            logger.Info($"Получено {stations.Count} пользовательских станций");

            return stations;
        }

        public Task<Station> CreateStationAsync(string sessionId, string title, string cover, string decription, long ownerId, string ownerName, string ownerPhoto)
        {
            logger.Info($"Создание пользовательской радиостанции с ID {sessionId}");

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

            IsStarted = true;

            return HttpRequestAsync<Station>("createStation", p);
        }

        public Task<bool> DeleteStationAsync(string sessionId)
        {
            logger.Info($"Удаление пользовательской радиостанции с ID {sessionId}");

            var p = new Dictionary<string, string>()
            {
                {"sessionId", sessionId }
            };

            IsStarted = false;

            return HttpRequestAsync<bool>("deleteStation", p);
        }

        public async Task<string> UploadCoverAsync(string path)
        {
            await using var stream = File.OpenRead(path);

            var data = new MultipartFormDataContent
            {
                { new StreamContent(stream), "image", "image" }
            };

            using var response = await backendConnectionService.Client.PostAsync("/radio/uploadImage", data);

            response.EnsureSuccessStatusCode();

            return $"{backendConnectionService.Host}/{await response.Content.ReadAsStringAsync()}";
        }

        private async Task<TResponse> HttpRequestAsync<TResponse>(string method, Dictionary<string, string> parameters)
        {
            try
            {
                var p = parameters.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value));

                return await backendConnectionService.Client.GetFromJsonAsync<TResponse>("/radio/" + method + "?" + string.Join("&", p));
            }
            catch(Exception ex)
            {
                logger.Info($"Произошла ошибка при запросе: {ex}");
                logger.Error(ex);
                throw;
            }
           
        }
    }
}
