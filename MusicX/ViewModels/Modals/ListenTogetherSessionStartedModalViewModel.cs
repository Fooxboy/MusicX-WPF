using MusicX.Services.Player;
using MusicX.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Common;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;

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
            var notificationService = StaticService.Container.GetRequiredService<NotificationsService>();
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            this.CopyUrlCommand = new RelayCommand(()=>
            {
                Clipboard.SetText(Url);

                notificationService.Show("Успешно скопировано", "Url адресс добавлен в буфер обмена!");
            });
            this.CopySessionCommand = new RelayCommand(() =>
            {
                Clipboard.SetText(SessionId);

                notificationService.Show("Успешно скопировано", "ID сессии добавлен в буфер обмена!");

            });

            this.CloseCommand = new RelayCommand(() =>
            {
                navigationService.CloseModal();
            });
        }
    }
}
