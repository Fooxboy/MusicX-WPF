using MusicX.Shared;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicX.Core.Services
{
    public class ServerService
    {
        private readonly Logger logger;
        public ServerService(Logger logger)
        {
            this.logger = logger;
        }
        private string urlUpdates = string.Empty;

        public void SetUrlUpdates(string url)
        {
            this.urlUpdates = url;
        }

        public async Task<CheckModel> CheckUpdates(string version)
        {
            try
            {
                CheckModel model;
                using (HttpClient client = new HttpClient())
                {
                    var res = await client.GetAsync(urlUpdates + $"/api/updates/check?version={version}");
                    var json = await res.Content.ReadAsStringAsync();

                    model = JsonSerializer.Deserialize<CheckModel>(json);
                }

                return model;
            }catch (Exception ex)
            {
                logger.Error("Error in server");
                logger.Error(ex, ex.Message);

                throw;
            }
            
        }
    }
}
