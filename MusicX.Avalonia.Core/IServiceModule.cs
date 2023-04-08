using Jab;
using MusicX.Avalonia.Core.Services;
using VkApi;
using VkApi.Core.Abstractions;
using VkApi.Core.Errors;

namespace MusicX.Avalonia.Core;

[ServiceProviderModule]
[Singleton<ConfigurationService>]
[Transient<Api>(Factory = nameof(CreateVkApi))]
[Singleton<FakeSafetyNetClient>]
[Singleton<ReceiptParser>]
[Singleton<IApiExceptionFactory, ApiExceptionFactory>]
[Transient<AuthService>]
public interface IServiceModule
{
    public Api CreateVkApi(ConfigurationService configurationService);
}