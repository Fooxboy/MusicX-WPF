using System.Reflection;
using Microsoft.Extensions.Logging;
using VkNet.Abstractions.Core;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils.AntiCaptcha;
namespace MusicX.Core.Services;

/// <inheritdoc />
public class CaptchaHandler : ICaptchaHandler
{
    private static readonly MethodInfo PerformAsyncMethod = typeof(CaptchaHandler).GetMethod(nameof(PerformAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    private readonly ICaptchaSolver _captchaSolver;
    private readonly ILogger<CaptchaHandler>? _logger;

    /// <inheritdoc cref="T:CaptchaHandler" />
    public CaptchaHandler(ILogger<CaptchaHandler>? logger, ICaptchaSolver captchaSolver)
    {
        _logger = logger;
        _captchaSolver = captchaSolver;
    }

    /// <inheritdoc />
    public int MaxCaptchaRecognitionCount { get; set; } = 3;

    /// <inheritdoc />
    public T? Perform<T>(Func<ulong?, string?, T> action)
    {
        if (typeof(T).IsAssignableTo(typeof(Task)))
        {
            return (T?)PerformAsyncMethod.MakeGenericMethod(typeof(T).GenericTypeArguments[0], typeof(T))
                .Invoke(this, new object[] {action});
        }
        
        var recognitionCount = MaxCaptchaRecognitionCount;
        var num = MaxCaptchaRecognitionCount + 1;
        var captchaSidTemp = new ulong?();
        string? captchaKeyTemp = null;
        var flag = false;
        var obj = default (T);
        do
        {
            try
            {
                obj = action(captchaSidTemp, captchaKeyTemp);
                --num;
                flag = true;
            }
            catch (CaptchaNeededException ex)
            {
                RepeatSolveCaptchaAsync(ex, ref recognitionCount, ref captchaSidTemp, ref captchaKeyTemp);
            }
        }
        while (recognitionCount > 0 && !flag);
        if (flag || !captchaSidTemp.HasValue)
            return obj;

        throw new CaptchaNeededException(new()
        {
            CaptchaSid = captchaSidTemp.Value
        });
    }

    private async Task<T> PerformAsync<T, T2>(Func<ulong?, string?, T2> action) where T2 : Task<T>
    {
        var recognitionCount = MaxCaptchaRecognitionCount;
        var num = MaxCaptchaRecognitionCount + 1;
        var captchaSidTemp = new ulong?();
        string? captchaKeyTemp = null;
        var flag = false;
        var obj = default (T);
        do
        {
            try
            {
                obj = await action(captchaSidTemp, captchaKeyTemp).ConfigureAwait(false);
                --num;
                flag = true;
            }
            catch (CaptchaNeededException ex)
            {
                RepeatSolveCaptchaAsync(ex, ref recognitionCount, ref captchaSidTemp, ref captchaKeyTemp);
            }
        }
        while (recognitionCount > 0 && !flag);
        if (flag || !captchaSidTemp.HasValue)
            return obj;

        throw new CaptchaNeededException(new()
        {
            CaptchaSid = captchaSidTemp.Value
        });
    }

    private void RepeatSolveCaptchaAsync(
        CaptchaNeededException captchaNeededException,
        ref int numberOfRemainingAttemptsToSolveCaptcha,
        ref ulong? captchaSidTemp,
        ref string? captchaKeyTemp)
    {
        _logger?.LogWarning("Повторная обработка капчи");
        if (numberOfRemainingAttemptsToSolveCaptcha < MaxCaptchaRecognitionCount)
            _captchaSolver?.CaptchaIsFalse();
        if (numberOfRemainingAttemptsToSolveCaptcha <= 0)
            return;
        captchaSidTemp = captchaNeededException.Sid;
        captchaKeyTemp = _captchaSolver?.Solve(captchaNeededException.Img?.AbsoluteUri);
        --numberOfRemainingAttemptsToSolveCaptcha;
    }
}
