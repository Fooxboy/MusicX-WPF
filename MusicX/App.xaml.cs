using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MusicX.Services;
using MusicX.Views;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace MusicX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            if(e.Args != null && e.Args.Length > 0)
            {
                if (!InstanceCheck())
                {
                    await SingleAppService.Instance.SendArguments(e.Args);
                    Application.Current.Shutdown();

                    return;
                }
            }

            base.OnStartup(e);

            AppCenter.Start("02130c6d-0a3b-4aa2-b46c-8aeb66c3fd71",
                   typeof(Analytics), typeof(Crashes));

            var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
            Analytics.TrackEvent("StartApp", properties);

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
