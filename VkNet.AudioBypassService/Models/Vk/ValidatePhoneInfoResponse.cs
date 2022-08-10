using Newtonsoft.Json;
namespace VkNet.AudioBypassService.Models.Vk;

public record ValidatePhoneInfoResponse([property: JsonProperty("callreset")] CallResetInfo CallReset);
public record CallResetInfo(int CodeLength, string PhoneTemplate);
