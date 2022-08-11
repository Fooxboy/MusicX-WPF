using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
namespace VkNet.AudioBypassService.Models.Vk;

public record ValidatePhoneResponse(
    string Sid, 
    PhoneConfirmationType ValidationType, 
    int CodeLength, 
    [property: JsonProperty("libverify_support")] bool? LibVerifySupport,
    [property: CanBeNull] string ExternalId,
    [property: CanBeNull] string Phone,
    PhoneConfirmationType? ValidationResend, 
    int? Delay);

public enum PhoneConfirmationType
{
    None,
    [EnumMember(Value = "callreset")]
    CallReset,
    [EnumMember(Value = "sms")]
    Sms
}
