using System.Threading.Tasks;
using VkNet.AudioBypassService.Abstractions;

namespace MusicX.Services.Stores;

public class ExchangeTokenStore : IExchangeTokenStore
{
    private readonly ConfigService _configService;

    public ExchangeTokenStore(ConfigService configService)
    {
        _configService = configService;
    }

    public ValueTask<string?> GetExchangeTokenAsync()
    {
        return new(_configService.Config.ExchangeToken);
    }

    public ValueTask SetExchangeTokenAsync(string? token)
    {
        _configService.Config.ExchangeToken = token;
        
        return new(_configService.SetConfig(_configService.Config));
    }
}