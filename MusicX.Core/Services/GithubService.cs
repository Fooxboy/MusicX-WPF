using MusicX.Core.Models.Github;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Services
{
    public class GithubService
    {
        private readonly Logger logger;
        public GithubService(Logger logger)
        {
            this.logger = logger;
        }

        public async Task<Release> GetLastRelease()
        {
            try
            {
                Release release;
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("user-agent", "musicx v1");
                    var res = await client.GetAsync("https://api.github.com/repos/fooxboy/musicxreleases/releases/latest");
                    var json = await res.Content.ReadAsStringAsync();

                    release = JsonConvert.DeserializeObject<Release>(json); 
                    
                }

                return release;
            }
            catch (Exception ex)
            {
                logger.Error("Error in github ");
                logger.Error(ex, ex.Message);

                throw;
            }

        }
    }
}
