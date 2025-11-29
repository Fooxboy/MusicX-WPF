using System.Reflection;
using Microsoft.Extensions.Logging;
using VkNet.Utils;

namespace VkNet.Extensions.DependencyInjection;

/// <inheritdoc />
public class AsyncCaptchaHandler : ICaptchaHandler
{
    private static readonly MethodInfo PerformAsyncMethod = typeof(AsyncCaptchaHandler).GetMethod(nameof(PerformAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    private readonly IAsyncCaptchaSolver? _captchaSolver;
    private readonly ILogger<CaptchaHandler>? _logger;

    /// <inheritdoc cref="T:CaptchaHandler" />
    public AsyncCaptchaHandler(ILogger<CaptchaHandler>? logger, IAsyncCaptchaSolver? captchaSolver)
    {
        _logger = logger;
        _captchaSolver = captchaSolver;
    }

    /// <inheritdoc />
    public int MaxCaptchaRecognitionCount { get; set; } = 3;

    /// <inheritdoc />
    public T Perform<T>(Func<CaptchaResponse?, T> action)
    {
        if (typeof(T).IsAssignableTo(typeof(Task)))
        {
            return (T)PerformAsyncMethod.MakeGenericMethod(typeof(T).GenericTypeArguments[0], typeof(T))
                .Invoke(this, [action])!;
        }
        
        var context = new CaptchaSolveContext(MaxCaptchaRecognitionCount);
        var flag = false;
        var obj = default (T);
        do
        {
            try
            {
                obj = action(context.Response);
                flag = true;
            }
            catch (CaptchaRequiredException ex)
            {
                if (!RepeatSolveCaptchaAsync(ex, context).Result)
                    break;
            }
        }
        while (context.RemainingSolveAttempts > 0 && !flag);
        if (flag || context.Response is null)
            return obj!;

        throw new CaptchaRequiredException(context.Response?.ToError() ?? new()
        {
            ErrorCode = 14
        });
    }

    private async Task<T> PerformAsync<T, T2>(Func<CaptchaResponse?, T2> action) where T2 : Task<T>
    {
        var context = new CaptchaSolveContext(MaxCaptchaRecognitionCount);
        var flag = false;
        var obj = default (T);
        do
        {
            try
            {
                obj = await action(context.Response);
                flag = true;
            }
            catch (CaptchaRequiredException ex)
            {
                if (!await RepeatSolveCaptchaAsync(ex, context))
                    break;
            }
        }
        while (context.RemainingSolveAttempts > 0 && !flag);
        if (flag || context.Response is null)
            return obj!;

        throw new CaptchaRequiredException(context.Response?.ToError() ?? new()
        {
            ErrorCode = 14
        });
    }
    
    private async Task<bool> RepeatSolveCaptchaAsync(
        CaptchaRequiredException exception,
        CaptchaSolveContext context)
    {
        _logger?.LogWarning("Повторная обработка капчи");
        if (context.RemainingSolveAttempts < MaxCaptchaRecognitionCount && _captchaSolver is not null)
            await _captchaSolver.SolveFailedAsync();
        if (context.RemainingSolveAttempts <= 0)
            return false;
        
        var task = exception.Error.RedirectUri is null
            ? _captchaSolver?.SolveAsync(new ImageCaptchaRequest(exception.Error.CaptchaImg))
            : _captchaSolver?.SolveAsync(new BrowserCaptchaRequest(exception.Error.RedirectUri));
        if (task.HasValue && await task.Value is { } response)
        {
            context.Response = exception.Error.RedirectUri is null
                ? new ImageCaptchaResponse(exception.Error.CaptchaSid, response)
                : new BrowserCaptchaResponse(exception.Error.CaptchaSid, response);
        }
        else
            context.Response = null;
        context.RemainingSolveAttempts--;
        return context.Response is not null;
    }

    private class CaptchaSolveContext
    {
        public int RemainingSolveAttempts;
        public CaptchaResponse? Response;

        public CaptchaSolveContext(int maxSolveAttempts)
        {
            RemainingSolveAttempts = maxSolveAttempts;
        }
    }
}