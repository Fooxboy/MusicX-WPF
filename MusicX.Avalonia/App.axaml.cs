using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Jab;
using MusicX.Avalonia.Audio.Services;
using MusicX.Avalonia.Core;
using MusicX.Avalonia.Core.Services;
using MusicX.Avalonia.Pages;
using MusicX.Avalonia.ViewModels.ViewModels;
using MusicX.Avalonia.Views;
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
[Singleton<MainWindowViewModel>]
[Transient<LoginModalViewModel>]
[Transient<SectionTabViewModel>]
[Singleton<PlaybackViewModel>]
[Transient<PlaylistViewModel>]
[Singleton<GlobalViewModel>]
[Transient<VideoModalViewModel>]
[Singleton<QueueViewModel>]
[Singleton<IPlayerService>(Factory = nameof(CreatePlayerService))]
[Singleton<IQueueService>(Factory = nameof(CreateQueueService))]
[Transient<MainWindow>]
[Transient<LoginPage>]
[Transient<SectionPage>]
[Transient<PlaylistPage>]
[Transient<VideoPage>]
[Transient<QueuePage>]
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

    public IPlayerService CreatePlayerService()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041) ? UseWindowsAudio() : UseBassAudio();
    }

    public IQueueService CreateQueueService()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041) ? UseWindowsQueue() : UseBassQueue();
    }

    private QueueService UseBassQueue() => new(GetService<IPlayerService>());

    private static IPlayerService UseBassAudio() => new PlayerService();
    private IPlayerService UseWindowsAudio() =>
#if WINDOWS
        new Audio.Windows.WindowsPlayerService();
#else
        UseBassAudio();
#endif

    private IQueueService UseWindowsQueue() =>
#if WINDOWS
        new Audio.Windows.WindowsQueueService(GetService<IPlayerService>());
#else
        UseBassQueue();
#endif

    public partial class Scope
    {
        public Api CreateVkApi(ConfigurationService configurationService) => _root.CreateVkApi(configurationService);
    }
}