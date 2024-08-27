using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MusicX.Patches;
using MusicX.Services;
using MusicX.Views;

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

            AppCenter.Start("02130c6d-0a3b-4aa2-b46c-8aeb66c3fd71",
                   typeof(Analytics), typeof(Crashes));

            var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
            Analytics.TrackEvent("StartApp", properties);
            
            ItemContainerGeneratorIndexHook.Apply();

            var window = new StartingWindow(e.Args);
            window.Show();

            await SingleAppService.Instance.StartArgsListener();
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
