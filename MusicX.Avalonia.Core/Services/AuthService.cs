using System.Text.Json;
using System.Text.Json.Nodes;
using MusicX.Avalonia.Core.Models;
using VkApi;
using VkApi.Core.Abstractions;
using VkApi.Core.Requests;

namespace MusicX.Avalonia.Core.Services;

public class AuthService : IDisposable
{
    private readonly Api _api;
    private readonly IApiExceptionFactory _exceptionFactory;
    private readonly ReceiptParser _receiptParser;

    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new("https://oauth.vk.com/")
    };

    public AuthService(Api api, IApiExceptionFactory exceptionFactory, ReceiptParser receiptParser)
    {
        _api = api;
        _exceptionFactory = exceptionFactory;
        _receiptParser = receiptParser;
    }

    public async Task<AuthTokenResponse> AuthAsync(string username, string password, string? code = null)
    {
        RefreshHeaders();
        
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", "2274003" },
            { "client_secret", "hHbZxrka2uZ6jB1inYsH" },
            { "2fa_supported", "1" },
            { "username", username },
            { "password", password },
            { "scope", "all" },
            { "device_id", RandomString.Generate(16) }
        };
        if (!string.IsNullOrEmpty(code))
            parameters["code"] = code;

        using var response = await _httpClient.PostAsync("token", new FormUrlEncodedContent(parameters));
        
        await using var stream = await response.Content.ReadAsStreamAsync();
        var node = JsonNode.Parse(stream)!;
        if (node["error"] != null)
            throw new AuthException(node.Deserialize(JsonContext.Default.AuthExceptionResponse)!, 
                                    node["error_code"]?.GetValue<int>() is { } errorCode ? _exceptionFactory.CreateExceptionFromCode(errorCode) : null);

        return node.Deserialize(JsonContext.Default.AuthTokenResponse)!;
    }

    public async Task<string> RefreshTokenAsync(string token)
    {
        var receipt = await _receiptParser.GetReceipt();
        
        _api.Client.Headers.Authorization = new("Bearer", token);
        
        var response =
            await _api.Client.RequestAsync<AuthRefreshTokenRequest, AuthRefreshTokenResponse>(
                "auth.refreshToken", new(receipt, null, null, null, null, null, null));
        
        return response.Token;
    }

    private void RefreshHeaders()
    {
        foreach (var (key, value) in _api.Client.Headers)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }

        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

public class AuthException : Exception
{
    public AuthExceptionResponse Data { get; }

    public AuthException(AuthExceptionResponse data, Exception? innerException = null) : base("An error occurred in authentication", innerException)
    {
        Data = data;
    }
}