using System;
using System.Threading.Tasks;
using VkNet.Extensions.DependencyInjection;

namespace MusicX.Services.Stores;

public class TokenStore : IVkTokenStore
{
    private readonly ConfigService _configService;

    public TokenStore(ConfigService configService)
    {
        _configService = configService;
    }

    public string Token =>
        _configService.Config.AccessToken ?? _configService.Config.AnonToken ??
        throw new InvalidOperationException("Authorization is required");

    public Task SetAsync(string? token, DateTimeOffset? expiration = null)
    {
        if (token?.StartsWith("anonym") == true)
        {
            _configService.Config.AnonToken = token;
        }
        else
        {
            _configService.Config.AccessToken = token;
            _configService.Config.AccessTokenTtl = expiration ?? DateTimeOffset.MaxValue;
        }
        
        return _configService.SetConfig(_configService.Config);
    }
}