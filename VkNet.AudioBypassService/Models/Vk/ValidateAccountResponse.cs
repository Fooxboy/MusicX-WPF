using System.Runtime.Serialization;
using JetBrains.Annotations;
namespace VkNet.AudioBypassService.Models.Vk;

public record ValidateAccountResponse(bool IsPhone, FlowType FlowName, [property: CanBeNull] string Sid);

public enum FlowType
{
    [EnumMember(Value = "need_registration")]
    NeedRegistration,
    [EnumMember(Value = "need_validation")]
    NeedValidation,
    [EnumMember(Value = "need_password_and_validation")]
    NeedPasswordAndValidation
}
