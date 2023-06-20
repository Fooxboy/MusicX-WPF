using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using MusicX.Models;
using NLog;
using Wpf.Ui.Contracts;

namespace MusicX.Services
{
    public class ConfigService
    {
        private readonly JsonSerializerOptions _configSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        
        private readonly Logger _logger;
        private readonly string _configPath;

        public ConfigService(Logger logger, ISnackbarService snackbarService)
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
                snackbarService.Show("Ошибка миграции!",
                    "Неудалось использовать настройки из прошлой установки приложения.");
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
            ConfigModel? config = null;
            if(File.Exists(_configPath))
            {
                await using var stream = File.OpenRead(_configPath);

                config = await JsonSerializer.DeserializeAsync<ConfigModel>(stream, _configSerializerOptions);
            }

            if (config is null)
                await SetConfig(config = new());

            Config = config;
            return config;
        }

        public async Task SetConfig(ConfigModel config)
        {
            Config = config;

            await using var stream = File.Create(_configPath);
            await JsonSerializer.SerializeAsync(stream, config, _configSerializerOptions);
        }
    }
}
