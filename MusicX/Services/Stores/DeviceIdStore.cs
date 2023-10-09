using System.Threading.Tasks;
using VkNet.AudioBypassService.Abstractions;

namespace MusicX.Services.Stores;

public class DeviceIdStore : IDeviceIdStore
{
    private readonly ConfigService _configService;

    public DeviceIdStore(ConfigService configService)
    {
        _configService = configService;
    }

    public ValueTask<string?> GetDeviceIdAsync()
    {
        return new(_configService.Config.DeviceId);
    }

    public ValueTask SetDeviceIdAsync(string deviceId)
    {
        _configService.Config.DeviceId = deviceId;
        return new(_configService.SetConfig(_configService.Config));
    }
}