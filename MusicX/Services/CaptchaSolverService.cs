using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Helpers;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using VkNet.Extensions.DependencyInjection;
using Wpf.Ui;

namespace MusicX.Services;

public class CaptchaSolverService : IAsyncCaptchaSolver
{
    private readonly NavigationService _navigationService;
    private readonly ISnackbarService _snackbarService;
    private readonly IServiceProvider _serviceProvider;

    public CaptchaSolverService(NavigationService navigationService, ISnackbarService snackbarService,
        IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _snackbarService = snackbarService;
        _serviceProvider = serviceProvider;
    }

    public ValueTask<string?> SolveAsync(string url)
    {
        var viewModel = _serviceProvider.GetRequiredService<CaptchaModalViewModel>();
        viewModel.ImageUrl = url;
        
        _navigationService.OpenModal<CaptchaModal>(viewModel);

        return new(viewModel.CompletionSource.Task);
    }

    public ValueTask SolveFailedAsync()
    {
        _snackbarService.ShowException("Ошибка!", "Вы ввели неправильную капчу");
        return ValueTask.CompletedTask;
    }
}