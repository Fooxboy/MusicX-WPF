using JetBrains.Annotations;
using Newtonsoft.Json;
namespace VkNet.AudioBypassService.Models.Vk;

public record ValidatePhoneConfirmResponse(string Sid, bool ProfileExists, bool CanSkipPassword, [property: CanBeNull] ValidatePhoneProfile Profile);

public record ValidatePhoneProfile(
    string FirstName,
    string LastName, 
    [property: JsonProperty("has_2fa")] bool HasTwoFactor,
    [property: JsonProperty("photo_200")] string Photo200,
    string Phone,
    bool CanUnbindPhone);
