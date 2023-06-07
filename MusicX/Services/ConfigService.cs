using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using MusicX.Models;
using Newtonsoft.Json;
using NLog;

namespace MusicX.Services
{
    public class ConfigService
    {
        private readonly Logger _logger;
        private readonly string _configPath;

        public ConfigService(Logger logger, NotificationsService notificationsService)
        {
            _logger = logger;
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MusicX");
            _configPath = Path.Combine(path, Name);

            if (Directory.Exists(path)) return;
            
            Directory.CreateDirectory(path);
            try
            {
                MigrateOldConfig(path);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to migrate config");
                notificationsService.Show("Ошибка миграции!", "Неудалось использовать настройки из прошлой установки приложения.");
            }
        }

        private void MigrateOldConfig(string newPath)
        {
            var subKey =
                Registry.LocalMachine.OpenSubKey(
                    "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MusicX");

            var oldPath = subKey?.GetValue("InstallLocation") as string ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MusicX");

            oldPath = Path.Combine(oldPath, Name);
            
            if (!File.Exists(oldPath))
                return;
            
            _logger.Info("Migrating config from {0} to {1}", oldPath, newPath);
            File.Copy(oldPath, newPath);
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
