using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using MusicX.Helpers;
using MusicX.Models;
using NLog;
using Wpf.Ui;

namespace MusicX.Services
{
    public class ConfigService
    {
        private static readonly Semaphore ConfigSemaphore = new(1, 1, "MusicX_ConfigSemaphore");
        
        private readonly JsonSerializerOptions _configSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        
        private readonly Logger _logger;
        private readonly string _configPath;

        public ConfigService(Logger logger, ISnackbarService snackbarService)
        {
            _logger = logger;
            _configPath = Path.Combine(StaticService.UserDataFolder.FullName, Name);

            if (StaticService.UserDataFolder.Exists) return;
            
            StaticService.UserDataFolder.Create();
            try
            {
                MigrateOldConfig(StaticService.UserDataFolder.FullName);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to migrate config");
                snackbarService.ShowException("Ошибка миграции!",
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
                await ConfigSemaphore.WaitOneAsync();
                
                try
                {
                    await using var stream = File.OpenRead(_configPath);
                    config = await JsonSerializer.DeserializeAsync<ConfigModel>(stream, _configSerializerOptions);
                }
                catch (JsonException e)
                {
                    _logger.Error(e, "Failed to read config");
                }
                finally
                {
                    ConfigSemaphore.Release();
                }
            }

            if (config is null)
                await SetConfig(config = new());

            Config = config;
            return config;
        }

        public async Task SetConfig(ConfigModel config)
        {
            Config = config;
            
            await ConfigSemaphore.WaitOneAsync();

            try
            {
                await using var stream = File.Create(_configPath);
                await JsonSerializer.SerializeAsync(stream, config, _configSerializerOptions);
            }
            catch (JsonException e)
            {
                _logger.Error(e, "Failed to write config");
            }
            finally
            {
                ConfigSemaphore.Release();
            }
        }
    }
}
