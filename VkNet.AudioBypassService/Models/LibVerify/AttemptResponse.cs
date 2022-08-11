using System.Runtime.Serialization;
namespace VkNet.AudioBypassService.Models.LibVerify;

public record AttemptResponse(LibVerifyStatus Status, int TokenExpirationTime, string Token);

public enum LibVerifyStatus
{
    NotOk,
    [EnumMember(Value = "OK")]
    Ok
}
