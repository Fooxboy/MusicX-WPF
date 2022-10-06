namespace VkNet.Extensions.DependencyInjection;

public class DefaultVkTokenStore : IVkTokenStore
{
    private string? _token;

    public string Token => _token ?? throw new InvalidOperationException("Authorization required");

    public Task SetAsync(string? token, DateTimeOffset? expiration = null)
    {
        _token = token;
        return Task.CompletedTask;
    }
}