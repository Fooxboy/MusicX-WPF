namespace VkNet.AudioBypassService.Abstractions;

public interface IDeviceIdProvider
{
    string DeviceId { get; }
    string Gaid { get; }
}
