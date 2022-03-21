using MusicX.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MusicX.Services
{
    public class ConfigService
    {
        private readonly string path = $"{AppDomain.CurrentDomain.BaseDirectory}";
        private readonly string name = "config.json";

        public async Task<ConfigModel> GetConfig()
        {
            if(File.Exists(path + name))
            {
                var file = await File.ReadAllTextAsync(path + name);

                var model = JsonConvert.DeserializeObject<ConfigModel>(file);

                return model;

            }else
            {
                var config = new ConfigModel();

                await SetConfig(config);

                return config;
            }
        }

        public async Task SetConfig(ConfigModel config)
        {
            var json= JsonConvert.SerializeObject(config);

            await File.WriteAllTextAsync(path + name, json);
        }
    }
}
