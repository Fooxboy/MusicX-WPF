using System;
using System.Threading;
using System.Windows;
using MusicX.Patches;
using MusicX.Services;
using MusicX.Views;
using NLog;
using Sentry;
using Sentry.NLog;

namespace MusicX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
#if !DEBUG
            Velopack.VelopackApp.Build().Run();
            
            if (!InstanceCheck())
            {
                if (e.Args != null && e.Args.Length > 0) //открытие не нового приложения, а передача агрументов уже в открытое
                {
                    await SingleAppService.Instance.SendArguments(e.Args);
                    Current.Shutdown();
                }

                return;
            }
#endif
            
            base.OnStartup(e);

            SetupAnalytics();
            
            if (ShowUnsupportedOsMessage())
                return;
            
            ItemContainerGeneratorIndexHook.Apply();

            var window = new StartingWindow(e.Args);
            window.Show();

            await SingleAppService.Instance.StartArgsListener();
        }

        private static bool ShowUnsupportedOsMessage()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(StaticService.MinimumOsVersion.Major,
                    StaticService.MinimumOsVersion.Minor, 
                    StaticService.MinimumOsVersion.Build,
                    StaticService.MinimumOsVersion.Revision))
                return false;

            if (OperatingSystem.IsWindowsVersionAtLeast(10))
            {
                var window = new UnsupportedOsVersionWindow();
                
                window.Show();
                return true;
            }
            
            MessageBox.Show(
                $"""
                 Установите последнюю версию Windows 10 или выше
                 Минимальная версия: {StaticService.MinimumOsVersion}
                 У вас установлена версия {StaticService.CurrentOsVersion}
                 """,
                "Неподдерживаемая версия Windows", MessageBoxButton.OK, MessageBoxImage.Error);
            
            return true;
        }

        private static void SetupAnalytics()
        {
            var sentryOptions = new SentryNLogOptions
            {
#if DEBUG
                Debug = true,
                Environment = "debug",
#else
                Environment = System.Version.TryParse(StaticService.Version, out _) ? "release" : "beta",
#endif
                Dsn = "https://4fa08f233778416a98210e27f558d049@glitchtip.zznty.ru/1",
                IsGlobalModeEnabled = true,
                AutoSessionTracking = true,
                StackTraceMode = StackTraceMode.Enhanced
            };

            SentrySdk.Init(sentryOptions);
            LogManager.Setup().SetupExtensions(b => b.RegisterTarget<SentryTarget>("Sentry"));

            const string sentryTargetName = "sentry";
            
            LogManager.Configuration.AddTarget(sentryTargetName, new SentryTarget(sentryOptions)
            {
                Name = sentryTargetName,
                Layout = "${message}"
            });
            LogManager.Configuration.AddRuleForAllLevels(sentryTargetName);
            
            LogManager.ReconfigExistingLoggers();
        }

        static Mutex? InstanceCheckMutex;
        static bool InstanceCheck()
        {
            var mutex = new Mutex(true, "MusicXPlayer", out var isNew);
            if (isNew)
                InstanceCheckMutex = mutex;
            else
                mutex.Dispose(); // отпустить mutex сразу
            return isNew;
        }
    }
}
