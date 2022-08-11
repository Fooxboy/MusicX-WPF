using System.Threading.Tasks;
using JetBrains.Annotations;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Models.Vk;
using VkNet.Utils;
namespace VkNet.AudioBypassService.Utils;

public sealed class BypassAuthCategory
{
    private IVkApiInvoker _invoker;
    private IDeviceIdProvider _idProvider;
    private IReceiptParser _receiptParser;
    public BypassAuthCategory(IVkApiInvoker invoker, IDeviceIdProvider idProvider, IReceiptParser receiptParser)
    {
        _invoker = invoker;
        _idProvider = idProvider;
        _receiptParser = receiptParser;
    }
    public Task<ValidatePhoneResponse> ValidatePhoneAsync(string phone, string anonToken, string apiId, [CanBeNull] string sid, bool isResend, bool allowCallReset = true, string flowType = null)
    {
        return _invoker.CallAsync<ValidatePhoneResponse>("auth.validatePhone", new()
        {
            {"lang", "ru"},
            {"api_id", apiId},
            {"access_token", anonToken},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"sid", sid},
            {"phone", phone},
            {"allow_callreset", allowCallReset},
            {"voice", isResend},
            {"flow_type", flowType}
        });
    }

    public Task<ValidatePhoneInfoResponse> ValidatePhoneInfoAsync(string phone, string anonToken, string apiId, string sid, string flowType = null)
    {
        return _invoker.CallAsync<ValidatePhoneInfoResponse>("auth.validatePhoneInfo", new()
        {
            {"lang", "ru"},
            {"api_id", apiId},
            {"access_token", anonToken},
            {"device_id", _idProvider.DeviceId},
            {"sid", sid},
            {"phone", phone},
            {"flow_type", flowType}
        });
    }

    public Task<ValidatePhoneConfirmResponse> ValidatePhoneConfirmAsync(string phone, string anonToken, string apiId, string sid, string code, string flowType = null)
    {
        return _invoker.CallAsync<ValidatePhoneConfirmResponse>("auth.validatePhoneConfirm", new()
        {
            {"lang", "ru"},
            {"api_id", apiId},
            {"access_token", anonToken},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"can_skip_password", true},
            {"sid", sid},
            {"phone", phone},
            {"code", code},
            {"flow_type", flowType}
        });
    }
    
    public Task<ValidatePhoneConfirmResponse> ValidatePhoneConfirmLibVerifyAsync(string phone, string anonToken, string apiId, string sid, string session, string token)
    {
        return _invoker.CallAsync<ValidatePhoneConfirmResponse>("auth.validatePhoneConfirm", new()
        {
            {"lang", "ru"},
            {"api_id", apiId},
            {"access_token", anonToken},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"can_skip_password", true},
            {"sid", sid},
            {"phone", phone},
            {"validate_session", session},
            {"validate_token", token}
        });
    }
    
    public Task<ValidateAccountResponse> ValidateAccountAsync(string phone, string anonToken, string apiId, bool forcePassword = false, string flowType = "auth_without_password")
    {
        return _invoker.CallAsync<ValidateAccountResponse>("auth.validateAccount", new()
        {
            {"lang", "ru"},
            {"api_id", apiId},
            {"access_token", anonToken},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"login", phone},
            {"force_password", forcePassword},
            {"flow_type", flowType}
        });
    }
    
    public async Task<string> RefreshTokenAsync(string oldToken)
    {
        var parameters = new VkParameters
        {
            { "receipt", await _receiptParser.GetReceiptAsync() },
            { "access_token", oldToken }
        };

        var response = await _invoker.CallAsync("auth.refreshToken", parameters);

        return response["token"]?.ToString();
    }
}
