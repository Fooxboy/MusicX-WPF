using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Helpers;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using VkNet.Extensions.DependencyInjection;
using Wpf.Ui;

namespace MusicX.Services;

public class CaptchaSolverService(
    NavigationService navigationService,
    ISnackbarService snackbarService,
    IServiceProvider serviceProvider)
    : IAsyncCaptchaSolver
{
    public ValueTask<string?> SolveAsync(CaptchaRequest request)
    {
        switch (request)
        {
            case BrowserCaptchaRequest { RedirectUri: var uri }:
            {
                var viewModel = serviceProvider.GetRequiredService<BrowserCaptchaModalViewModel>();
                viewModel.RedirectUri = uri;

                navigationService.OpenModal<BrowserCaptchaModal>(viewModel);

                return new(viewModel.CompletionSource.Task);
            }
            case ImageCaptchaRequest { ImageUri: var uri }:
            {
                var viewModel = serviceProvider.GetRequiredService<CaptchaModalViewModel>();
                viewModel.ImageUri = uri;

                navigationService.OpenModal<CaptchaModal>(viewModel);

                return new(viewModel.CompletionSource.Task);
            }
            default:
                return ValueTask.FromResult<string?>(null);
        }
    }

    public ValueTask SolveFailedAsync()
    {
        snackbarService.ShowException("Ошибка!", "Вы ввели неправильную капчу");
        return ValueTask.CompletedTask;
    }
}