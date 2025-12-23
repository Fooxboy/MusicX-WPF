using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.System.Profile;
using VkNet.Extensions.Auth.Abstractions;

namespace MusicX.Services;

public class WindowsDeviceIdProvider : IDeviceIdProvider
{
    public ValueTask<string> GetDeviceIdAsync()
    {
        return ValueTask.FromResult(
            Convert.ToHexStringLower(SystemIdentification.GetSystemIdForPublisher().Id.ToArray()));
    }
}