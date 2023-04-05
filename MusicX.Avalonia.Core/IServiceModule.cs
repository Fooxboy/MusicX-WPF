using Jab;
using MusicX.Avalonia.Core.Services;
using MusicX.Avalonia.Core.ViewModels;
using VkApi;
using VkApi.Core.Abstractions;
using VkApi.Core.Errors;

namespace MusicX.Avalonia.Core;

[ServiceProviderModule]
[Transient<MainWindowViewModel>]
[Singleton<ConfigurationService>]
[Transient<Api>(Factory = nameof(CreateVkApi))]
[Transient<LoginModalViewModel>]
[Singleton<FakeSafetyNetClient>]
[Singleton<ReceiptParser>]
[Singleton<IApiExceptionFactory, ApiExceptionFactory>]
[Transient<AuthService>]
public interface IServiceModule
{
    public Api CreateVkApi(ConfigurationService configurationService);
}