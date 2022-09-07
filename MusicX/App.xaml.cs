using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
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

        }
    }
}
