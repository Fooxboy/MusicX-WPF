using System.Collections.Generic;
namespace VkNet.AudioBypassService.Models.LibVerify;

public record FetcherInfo(
    int Timeout,
    string Status,
    string Url
);

public record VerifyResponse(
    string SessionId,
    string VerificationUrl,
    IReadOnlyList<string> Checks,
    string PushTemplate,
    string IvrTimeoutSec,
    string Status,
    string SmsTemplate,
    string CodeType,
    int CodeLength,
    string Operator,
    long ServerTimestamp,
    IReadOnlyList<string> CallTemplate,
    string ModifiedPhoneNumber,
    FetcherInfo FetcherInfo,
    string Service,
    int WaitForRoute,
    IReadOnlyList<object> SupportedIvrLanguages
);


