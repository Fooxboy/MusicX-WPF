using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Models.LibVerify;
using VkNet.Utils;
namespace VkNet.AudioBypassService.Utils;

public sealed class LibVerifyService : IDisposable
{
    private const string ServerKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA6FwwmlVnQhyEp%2BNi84d%2BcTFtg%2Fble%2BywLIkmKdHpBfyfO6nv4WCSClrf63AjRwCoXuCBiu42MFEnamr%2F7YlGeZ1MJjis0rMwtIgYpl4iLQRCYrafxZ1YH1sEzZVz3TtejLN5sCujsl5zZqQK7WhI%2BnybnPam72NHn%2BUVsYydFD%2BMHW2hYak5CtAsOMEU6nsxnV2Dp%2FxziHOKIsHnSys%2FUCSB1tN5X%2BTlYAn7xOKHHL23KusC9AOTj1B8pX4RMexQmNUFWmdLY61LBQMvrPcDQzXAn2LYideBGF4Ju0MoXMihKL4g14gCbIPg0nfJPwxjkDlJVxG03JfvJwXMIZP83wIDAQAB";
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri("https://clientapi.mail.ru/fcgi-bin"),
        DefaultRequestHeaders =
        {
            {"User-Agent", "Dalvik/2.1.0 (Linux; U; Android 12; MusicX_armeabi-v7a Build/S2B2.211203.006)"}
        }
    };

    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy()
    };
    private IDeviceIdProvider _idProvider;
    public LibVerifyService(IDeviceIdProvider idProvider)
    {
        _idProvider = idProvider;
    }

    public Task<VerifyResponse> VerifyAsync(string application, string applicationId, string authType, string service, string sessionId, string externalId, string phone)
    {
        var args = new VkParameters
        {
            {"application", application},
            {"application_id", applicationId},
            {"session_id", sessionId},
            {"language", "ru_RU"},
            {"phone", phone},
            {"platform", "android"},
            {"auth_type", authType},
            {"signature", "399be41b5872d510170661e1d16ceca3"},
            {"capabilities", new List<string>
            {
                "call_number_fragment",
                "call_session_hash",
                "background_verify",
                "ping_v2",
                "request_id",
                "safety_net_v3",
                "sms_retriever"
            }},
            {"checks", new List<string>
            {
                "callui",
                "sms",
                "push"
            }},
            {"device_id", _idProvider.Gaid},
            {"device_name", "Google+MusicX_armeabi-v7a"},
            {"env", "gps"},
            {"libverify_build", 228},
            {"libverify_version", "2.0"},
            {"mode", "manual"},
            {"os_version", 12},
            {"request_id", RandomString.Generate(32)},
            {"service", service},
            {"system_id", RandomString.Generate(16)},
            {"version", 13617},
            {"server_key", ServerKey},
            {"external_id", externalId}
        };
        var url = Url.Combine("/verify?", Url.QueryFrom(args.ToArray()));
        return _client.GetFromJsonAsync<VerifyResponse>(url, _options);
    }

    public Task<AttemptResponse> AttemptAsync(string application, string applicationId, string service, string sessionId, string code, string phone)
    {
        var args = new VkParameters
        {
            {"application", application},
            {"application_id", applicationId},
            {"code", code},
            {"code_source", "USER_INPUT"},
            {"id", sessionId},
            {"internal", "verify"},
            {"language", "ru_RU"},
            {"phone", phone},
            {"platform", "android"},
            {"service", service},
            {"signature", "399be41b5872d510170661e1d16ceca3"}
        };
        var url = Url.Combine("/attempt?", Url.QueryFrom(args.ToArray()));
        return _client.GetFromJsonAsync<AttemptResponse>(url, _options);
    }
    public void Dispose()
    {
        _client.Dispose();
    }
}
