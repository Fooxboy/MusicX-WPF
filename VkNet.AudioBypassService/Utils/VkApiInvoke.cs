#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions;
using VkNet.Abstractions.Core;
using VkNet.AudioBypassService.Abstractions;
using VkNet.Exception;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.JsonConverter;

namespace VkNet.AudioBypassService.Utils;

public class VkApiInvoke : IVkApiInvoke
{
    private readonly JsonSerializer _serializer = JsonSerializer.Create(new()
    {
        Converters = new JsonConverter[]
        {
            new VkCollectionJsonConverter(),
            new UnixDateTimeConverter(),
            new AttachmentJsonConverter(),
            new StringEnumConverter(),
        },
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        MaxDepth = null,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    });

    private readonly HttpClient _client;
    private readonly ICaptchaHandler _handler;
    private readonly IVkApiVersionManager _versionManager;
    private readonly IVkTokenStore _tokenStore;
    private readonly ILanguageService _languageService;
    private readonly IAsyncRateLimiter _rateLimiter;
    private readonly IDeviceIdStore _deviceIdStore;
    private readonly ITokenRefreshHandler _tokenRefreshHandler;

    public VkApiInvoke(HttpClient client, ICaptchaHandler handler, IVkApiVersionManager versionManager,
                       IVkTokenStore tokenStore, ILanguageService languageService, IAsyncRateLimiter rateLimiter, IDeviceIdStore deviceIdStore, ITokenRefreshHandler tokenRefreshHandler)
    {
        _client = client;
        _handler = handler;
        _versionManager = versionManager;
        _tokenStore = tokenStore;
        _languageService = languageService;
        _rateLimiter = rateLimiter;
        _deviceIdStore = deviceIdStore;
        _tokenRefreshHandler = tokenRefreshHandler;
    }

    private async ValueTask TryAddRequiredParameters(IDictionary<string, string> parameters, bool skipAuthorization)
    {
        parameters.TryAdd("v", _versionManager.Version);
        parameters.TryAdd("lang", _languageService.GetLanguage()?.ToString() ?? "ru");
        
        if (await _deviceIdStore.GetDeviceIdAsync() is { } deviceId)
            parameters.TryAdd("device_id", deviceId);
        
        if (!skipAuthorization)
            parameters.TryAdd("access_token", _tokenStore.Token);
    }
    
    public VkResponse Call(string methodName, VkParameters parameters, bool skipAuthorization = false)
    {
        return CallAsync(methodName, parameters, skipAuthorization).GetAwaiter().GetResult();
    }

    public T? Call<T>(string methodName, VkParameters parameters, bool skipAuthorization = false,
                      params JsonConverter[] jsonConverters)
    {
        return CallAsync<T>(methodName, parameters, skipAuthorization, jsonConverters).GetAwaiter().GetResult();
    }

    private async Task<T?> CallAsync<T>(string methodName, VkParameters parameters, bool skipAuthorization, JsonConverter[] jsonConverters)
    {
        if (jsonConverters.Length > 0)
            throw new NotSupportedException("Custom JsonConverters are not supported");

        await TryAddRequiredParameters(parameters, skipAuthorization);

        return await _handler.Perform(async (sid, key) =>
        {
            if (sid is { } captchaSid)
            {
                parameters.Add("captcha_sid", captchaSid.ToString());
                parameters.Add("captcha_key", key);
            }

            await _rateLimiter.WaitNextAsync();

            using var response = await _client.SendAsync(new()
            {
                Method = HttpMethod.Post,
                RequestUri = new(methodName, UriKind.Relative),
                Content = new FormUrlEncodedContent(parameters),
            }, HttpCompletionOption.ResponseHeadersRead);
            LastInvokeTime = DateTimeOffset.Now;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var textReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var reader = new JsonTextReader(textReader) { CloseInput = false };

            var obj = await JToken.ReadFromAsync(reader);

            if (obj["error"] is not { } error)
                return obj["response"]!.ToObject<T>(_serializer);

            var vkError = error.ToObject<VkError>(_serializer);

            if (vkError?.ErrorCode is not (5 or 1117 or 1114) || // token has expired
                await _tokenRefreshHandler.RefreshTokenAsync(_tokenStore.Token) is not { } newToken)
                throw new VkApiException(vkError);

            parameters["access_token"] = newToken;
            return await CallAsync<T>(methodName, parameters, skipAuthorization);
        });
    }

    public async Task<VkResponse> CallAsync(string methodName, VkParameters parameters, bool skipAuthorization = false)
    {
        var json = await InvokeInternalAsync(methodName, parameters, skipAuthorization);

        return new(json)
        {
            RawJson = json.ToString()
        };
    }

    public Task<T?> CallAsync<T>(string methodName, VkParameters parameters, bool skipAuthorization = false)
    {
        return CallAsync<T>(methodName, parameters, skipAuthorization, Array.Empty<JsonConverter>());
    }

    public string Invoke(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false)
    {
        return InvokeAsync(methodName, parameters, skipAuthorization).GetAwaiter().GetResult();
    }

    public async Task<string> InvokeAsync(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false)
    {
        var json = await InvokeInternalAsync(methodName, parameters, skipAuthorization);
        return json.ToString();
    }

    private async Task<JToken> InvokeInternalAsync(string methodName, IDictionary<string, string> parameters, bool skipAuthorization)
    {
        await TryAddRequiredParameters(parameters, skipAuthorization);
        
        return await _handler.Perform(async (sid, key) =>
        {
            if (sid is { } captchaSid)
            {
                parameters.Add("captcha_sid", captchaSid.ToString());
                parameters.Add("captcha_key", key);
            }

            await _rateLimiter.WaitNextAsync();

            using var response = await _client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(methodName, UriKind.Relative),
                Content = new FormUrlEncodedContent(parameters),
            }, HttpCompletionOption.ResponseHeadersRead);
            LastInvokeTime = DateTimeOffset.Now;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var textReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var reader = new JsonTextReader(textReader) { CloseInput = false };

            var obj = await JToken.ReadFromAsync(reader);

            if (obj["error"] is not { } error)
                return obj["response"]!;

            var vkError = error.ToObject<VkError>();

            if (vkError?.ErrorCode is not (5 or 1117 or 1114) || // token has expired
                await _tokenRefreshHandler.RefreshTokenAsync(_tokenStore.Token) is not { } newToken)
                throw new VkApiException(vkError);

            parameters["access_token"] = newToken;
            return await InvokeInternalAsync(methodName, parameters, skipAuthorization);
        });
    }

    public DateTimeOffset? LastInvokeTime { get; private set;}
    public TimeSpan? LastInvokeTimeSpan => DateTimeOffset.Now - LastInvokeTime;

    public record ResponseRecord<T>(T? Response, VkError? Error);
}