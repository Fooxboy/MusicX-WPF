using JetBrains.Annotations;
using Newtonsoft.Json;

namespace VkNet.AudioBypassService.Models.Ecosystem;

public record EcosystemSendOtpSmsResponse(int Status, string Sid, string Info, int CodeLength);

public record EcosystemCheckOtpResponse(string Sid, bool ProfileExist, bool CanSkipPassword, EcosystemProfile Profile);

public record EcosystemProfile(string FirstName, string LastName, string Phone, bool Has2Fa, bool CanUnbindPhone, [CanBeNull] [property: JsonProperty("photo_200")] string Photo200);