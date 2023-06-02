using MusicX.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MusicX.Services
{
    public class ConfigService
    {
        private readonly string _configPath;

        public ConfigService()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MusicX");
            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            _configPath = Path.Combine(path, Name);
        }

        private const string Name = "config.json";

        public ConfigModel Config { get; private set; } = null!;

        public async Task<ConfigModel> GetConfig()
        {
            if(File.Exists(_configPath))
            {
                var file = await File.ReadAllTextAsync(_configPath);

                var model = JsonConvert.DeserializeObject<ConfigModel>(file)!;

                Config = model;
                return model;

            }else
            {
                var config = new ConfigModel();

                await SetConfig(config);

                Config = config;
                return config;
            }
        }

        public async Task SetConfig(ConfigModel config)
        {
            Config = config;
            var json= JsonConvert.SerializeObject(config);

            await File.WriteAllTextAsync(_configPath, json);
        }
    }
}
