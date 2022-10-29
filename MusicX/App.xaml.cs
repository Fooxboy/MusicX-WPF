using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MusicX.Services;
using System.Collections.Generic;
using System.Linq;
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
            base.OnStartup(e);
            AppCenter.Start("02130c6d-0a3b-4aa2-b46c-8aeb66c3fd71",
                   typeof(Analytics), typeof(Crashes));

            var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
            Analytics.TrackEvent("StartApp", properties);

            if(e.Args.Length > 0) MessageBox.Show(e.Args[0]);

        }
    }
}
