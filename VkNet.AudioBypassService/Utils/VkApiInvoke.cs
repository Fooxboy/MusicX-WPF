using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using ICaptchaHandler = VkNet.Extensions.DependencyInjection.ICaptchaHandler;

namespace VkNet.AudioBypassService.Utils;

public class VkApiInvoke(
    HttpClient client,
    ICaptchaHandler handler,
    IVkApiVersionManager versionManager,
    IVkTokenStore tokenStore,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    IDeviceIdProvider deviceIdProvider,
    ITokenRefreshHandler tokenRefreshHandler)
    : IVkApiInvoke
{
    private const string AuthorizationScheme = "Bearer";

    internal static readonly JsonSerializer Serializer = JsonSerializer.Create(new()
    {
        Converters =
        [
            new VkCollectionJsonConverter(),
            new UnixDateTimeConverter(),
            new AttachmentJsonConverter(),
            new StringEnumConverter()
        ],
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        MaxDepth = null,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    });

    private async ValueTask TryAddRequiredParameters(IDictionary<string, string> parameters)
    {
        parameters.TryAdd("v", versionManager.Version);
        parameters.TryAdd("lang", languageService.GetLanguage()?.ToString() ?? "ru");
        parameters.TryAdd("device_id", await deviceIdProvider.GetDeviceIdAsync());
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

        await TryAddRequiredParameters(parameters);

        return await handler.Perform(async captchaResponse =>
        {
            var requestParameters = new VkParameters(parameters);
            captchaResponse?.AddTo(requestParameters);

            await rateLimiter.WaitNextAsync();
            
            Debug.WriteLine($"Request {methodName} {string.Join(", ", parameters.Select(b => $"{b.Key}={b.Value}"))}");

            using var response = await client.SendAsync(new()
            {
                Method = HttpMethod.Post,
                RequestUri = new(methodName, UriKind.Relative),
                Content = new FormUrlEncodedContent(requestParameters),
                Headers =
                {
                    Authorization = skipAuthorization ? null : new(AuthorizationScheme, tokenStore.Token)
                }
            }, HttpCompletionOption.ResponseHeadersRead);
            LastInvokeTime = DateTimeOffset.Now;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var textReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var reader = new JsonTextReader(textReader) { CloseInput = false };

            var obj = await JToken.ReadFromAsync(reader);
            
            Debug.WriteLine($"Response {methodName} {obj}");

            if (obj["error"] is not { } error)
                return obj["response"]!.ToObject<T>(Serializer);

            var vkError = error.ToObject<VkError>(Serializer);

            if (vkError?.ErrorCode is not (4 or 5 or 1117 or 1114) || // token has expired
                await tokenRefreshHandler.RefreshTokenAsync(tokenStore.Token) is null)
            {
                throw CreateApiError(vkError);
            }

            return await CallAsync<T>(methodName, requestParameters, skipAuthorization);
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
        await TryAddRequiredParameters(parameters);
        
        return await handler.Perform(async captchaResponse =>
        {
            var requestParameters = new Dictionary<string, string>(parameters);
            captchaResponse?.AddTo(requestParameters);

            await rateLimiter.WaitNextAsync();
            
            Debug.WriteLine($"Request {methodName} {string.Join(", ", parameters.Select(b => $"{b.Key}={b.Value}"))}");

            using var response = await client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(methodName, UriKind.Relative),
                Content = new FormUrlEncodedContent(requestParameters),
                Headers =
                {
                    Authorization = skipAuthorization ? null : new(AuthorizationScheme, tokenStore.Token)
                }
            }, HttpCompletionOption.ResponseHeadersRead);
            LastInvokeTime = DateTimeOffset.Now;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var textReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var reader = new JsonTextReader(textReader) { CloseInput = false };

            var obj = await JToken.ReadFromAsync(reader);
            
            Debug.WriteLine($"Response {methodName} {obj}");

            if (obj["error"] is not { } error)
                return obj["response"]!;

            var vkError = error.ToObject<VkError>();

            if (vkError?.ErrorCode is not (5 or 1117 or 1114) || // token has expired
                await tokenRefreshHandler.RefreshTokenAsync(tokenStore.Token) is null)
            {
                throw CreateApiError(vkError);
            }

            return await InvokeInternalAsync(methodName, requestParameters, skipAuthorization);
        });
    }

    private static VkApiMethodInvokeException CreateApiError(VkError? error)
    {
        if (error is null) return new VkApiMethodInvokeException(error);
        return error.ErrorCode == 14 ? new CaptchaRequiredException(error) : VkErrorFactory.Create(error);
    }

    public DateTimeOffset? LastInvokeTime { get; private set;}
    public TimeSpan? LastInvokeTimeSpan => DateTimeOffset.Now - LastInvokeTime;
}