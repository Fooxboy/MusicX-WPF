using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Jab;
using MusicX.Avalonia.Core;
using MusicX.Avalonia.Core.Models;
using MusicX.Avalonia.Core.Services;
using MusicX.Avalonia.Core.ViewModels;
using MusicX.Avalonia.Pages;
using MusicX.Avalonia.Views;
using ReactiveUI;
using VkApi;

namespace MusicX.Avalonia;

public partial class App : Application
{
    internal static ServiceProvider Provider { get; } = new();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Avalonia);
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Provider.GetService<MainWindow>();
            desktop.MainWindow.DataContext = Provider.GetService<MainWindowViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

[ServiceProvider]
[Import<IServiceModule>]
[Transient<MainWindow>]
[Transient<LoginPage>]
[Transient<SectionPage>]
internal partial class ServiceProvider : IServiceModule
{
    public Api CreateVkApi(ConfigurationService configurationService)
    {
        var state = configurationService.LoginState;
        var vkApi = new Api(state?.AccessToken ?? string.Empty, "5.160");
        if (state is null)
            vkApi.Client.Headers.Authorization = null;
        
        vkApi.Client.Headers.UserAgent.ParseAdd("VKAndroidApp/7.37-13617 (Android 12; SDK 32; armeabi-v7a; MusicX; ru; 2960x1440)");
        vkApi.Client.Headers.Add("X-VK-Android-Client", "new");
        
        return vkApi;
    }

    public partial class Scope
    {
        public Api CreateVkApi(ConfigurationService configurationService) => _root.CreateVkApi(configurationService);
    }
}