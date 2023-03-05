using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using VkNet.Extensions.DependencyInjection;

namespace MusicX.Services;

public class CaptchaSolverService : IAsyncCaptchaSolver
{
    private readonly NavigationService _navigationService;
    private readonly NotificationsService _notificationsService;
    private readonly IServiceProvider _serviceProvider;

    public CaptchaSolverService(NavigationService navigationService, NotificationsService notificationsService, IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _notificationsService = notificationsService;
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
        _notificationsService.Show("Ошибка!", "Вы ввели неправильную капчу");
        return ValueTask.CompletedTask;
    }
}