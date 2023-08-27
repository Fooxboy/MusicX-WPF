using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Services;
using Wpf.Ui;
using Wpf.Ui.Common;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.ViewModels.Modals
{
    public class ListenTogetherSessionStartedModalViewModel : BaseViewModel
    {
        private readonly ListenTogetherService _service;
        public string SessionId => _service.SessionId;

        public string Url => _service.ConnectUrl;

        public ICommand CopyUrlCommand { get; set; }
        public ICommand CopySessionCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        public ListenTogetherSessionStartedModalViewModel(ListenTogetherService service)
        {
            _service = service;
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            this.CopyUrlCommand = new RelayCommand(()=>
            {
                Clipboard.SetText(Url);

                snackbarService.Show("Успешно скопировано", "Url адресс добавлен в буфер обмена!");
            });
            this.CopySessionCommand = new RelayCommand(() =>
            {
                Clipboard.SetText(SessionId);

                snackbarService.Show("Успешно скопировано", "ID сессии добавлен в буфер обмена!");

            });

            this.CloseCommand = new RelayCommand(() =>
            {
                navigationService.CloseModal();
            });
        }
    }
}
