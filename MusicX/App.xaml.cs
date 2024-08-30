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
            
            ItemContainerGeneratorIndexHook.Apply();

            var window = new StartingWindow(e.Args);
            window.Show();

            await SingleAppService.Instance.StartArgsListener();
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
                Dsn = "https://44305d462f604317a8f81e7b170eb025@glitchtip.zznty.ru/1",
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
