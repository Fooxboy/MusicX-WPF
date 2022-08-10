using System.Runtime.Serialization;
namespace VkNet.AudioBypassService.Models.Vk;

public record ValidatePhoneResponse(string Sid, PhoneConfirmationType ValidationType, int CodeLength, PhoneConfirmationType? ValidationResend, int? Delay);

public enum PhoneConfirmationType
{
    [EnumMember(Value = "callreset")]
    CallReset,
    [EnumMember(Value = "sms")]
    Sms
}
