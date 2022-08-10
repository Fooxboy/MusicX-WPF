using System;
using VkNet.AudioBypassService.Abstractions;
namespace VkNet.AudioBypassService.Utils;

public class RandomDeviceIdProvider : IDeviceIdProvider
{
    public string DeviceId { get; }
    public string Gaid { get; }

    public RandomDeviceIdProvider()
    {
        DeviceId = RandomString.Generate(49);
        Gaid = Guid.NewGuid().ToString();
    }
}
