namespace VkNet.Extensions.DependencyInjection;

/// <summary>
/// Определяет интерфейс взаимодействия с сервисом-распознавателем капчи
/// </summary>
public interface IAsyncCaptchaSolver
{
    ValueTask<string?> SolveAsync(CaptchaRequest request);

    /// <summary>
    /// Сообщает сервису, что последняя капча была распознана неверно.
    /// </summary>
    ValueTask SolveFailedAsync();
}

public abstract record CaptchaRequest;
public record ImageCaptchaRequest(Uri ImageUri) : CaptchaRequest;
public record BrowserCaptchaRequest(Uri RedirectUri) : CaptchaRequest;