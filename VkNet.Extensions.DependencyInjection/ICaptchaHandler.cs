using VkNet.Model;
using VkNet.Utils;

namespace VkNet.Extensions.DependencyInjection;

/// <summary>Обработчик капчи</summary>
public interface ICaptchaHandler
{
    /// <summary>
    /// Максимальное количество попыток распознавания капчи c помощью
    /// зарегистрированного обработчика
    /// </summary>
    int MaxCaptchaRecognitionCount { get; set; }

    /// <summary>Обработка капчи</summary>
    /// <param name="action"> Действие </param>
    /// <typeparam name="T"> Тип результата </typeparam>
    /// <returns> Результат действия </returns>
    T Perform<T>(Func<CaptchaResponse?, T> action);
}

public abstract record CaptchaResponse
{
    public abstract VkError ToError();

    public abstract void AddTo(IDictionary<string, string> parameters);
}
public record ImageCaptchaResponse(ulong Sid, string Key) : CaptchaResponse
{
    public override VkError ToError() => new()
    {
        CaptchaSid = Sid
    };

    public override void AddTo(IDictionary<string, string> parameters)
    {
        parameters.Add("captcha_sid", Sid.ToString());
        parameters.Add("captcha_key", Key);
    }
}

public record BrowserCaptchaResponse(string SuccessToken) : CaptchaResponse
{
    public override VkError ToError() => new()
    {
        ErrorCode = 14,
        ErrorMessage = "Browser captcha has been failed"
    };

    public override void AddTo(IDictionary<string, string> parameters)
    {
        parameters.Add("success_token", SuccessToken);
    }
}