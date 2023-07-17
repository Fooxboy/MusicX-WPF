using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicX.ViewModels.Modals
{
    public class CreateUserRadioModalViewModel : BaseViewModel
    {
        public string TitleRadio { get; set; }

        public string DescriptionRadio { get; set; }

        public ICommand SelectRadioCoverCommand { get; set; }

        public ICommand CreateRadioCommand { get; set; }

        public string CoverPath { get; set; }

        public bool IsLoading { get; set; }

        public CreateUserRadioModalViewModel()
        {
            SelectRadioCoverCommand = new AsyncCommand(SelectRadioCover);
            CreateRadioCommand = new AsyncCommand(CreateRadio);
        }

        private async Task SelectRadioCover()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения ( *.jpg)|*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                CoverPath = openFileDialog.FileName;
            }
        }

        private async Task CreateRadio()
        {
            var radioService = StaticService.Container.GetRequiredService<UserRadioService>();
            var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();
            var configService = StaticService.Container.GetRequiredService<ConfigService>();
            var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();
            var logger = StaticService.Container.GetRequiredService<Logger>();

            if(string.IsNullOrEmpty(TitleRadio))
            {
                notificationsService.Show("Ошибка", "Вы не заполнили название");
                return;
            }

            if (string.IsNullOrEmpty(DescriptionRadio))
            {
                notificationsService.Show("Ошибка", "Вы не заполнили описание");
                return;
            }

            try
            {
                IsLoading = true;

                var config = await configService.GetConfig();
                var session = await listenTogetherService.StartSessionAsync(config.UserId);

                var station = await radioService.CreateStationAsync(session,
                    TitleRadio,
                    "https://as2.ftcdn.net/v2/jpg/02/87/95/77/1000_F_287957705_kVUIWM8TnTbavhGX9JTEAQLGQo6fVrc5.jpg", 
                    DescriptionRadio,
                    config.UserId, 
                    config.UserName, "photo");

                IsLoading = false;
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                notificationsService.Show("Ошибка", "Мы не смогли создать радиостанцию");
            }
        }
    }
}
