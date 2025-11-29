using System.Threading.Tasks;

namespace VkNet.AudioBypassService.Abstractions;

public interface IDeviceIdProvider
{
    ValueTask<string> GetDeviceIdAsync();
}