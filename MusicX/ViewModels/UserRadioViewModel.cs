using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Shared.ListenTogether.Radio;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicX.ViewModels
{
    public class UserRadioViewModel : BaseViewModel
    {
        public ObservableCollection<Station> Developers { get; set; }

        public ObservableCollection<Station> Recommended { get; set; }

        public ObservableCollection<Station> UserStations { get; set; }

        public ICommand ConnectToStationCommand { get; set; }

        public ICommand CreateStationCommand { get; set; }

        public UserRadioViewModel()
        {
            Developers = new ObservableCollection<Station>();
            Recommended = new ObservableCollection<Station>();
            UserStations = new ObservableCollection<Station>();

            ConnectToStationCommand = new AsyncCommand<Station>(ConnectToStation);
            CreateStationCommand = new AsyncCommand(CreateStation);
        }


        public async Task LoadData()
        {
            var radioService = StaticService.Container.GetRequiredService<UserRadioService>();
            Developers.Clear();
            Recommended.Clear();
            UserStations.Clear();

            try
            {
                var stations = await radioService.GetStationsList();

                foreach (var station in stations)
                {
                    if (station.Owner.OwnerCategory == OwnerCategory.Developer)
                    {
                        Developers.Add(station);
                    }

                    if (station.Owner.OwnerCategory == OwnerCategory.Recoms)
                    {
                        Recommended.Add(station);
                    }

                    if (station.Owner.OwnerCategory == OwnerCategory.User)
                    {
                        UserStations.Add(station);
                    }
                }
            }
            catch (Exception ex)
            {
                var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex);
                notificationsService.Show("Ошибка", "Мы не смогли загрузить список станций пользователей");

                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
            }
        }

        private async Task ConnectToStation(Station? station)
        {
            var userRadioService = StaticService.Container.GetRequiredService<UserRadioService>();
            var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();
            var configService = StaticService.Container.GetRequiredService<ConfigService>();

            try
            {

                if(userRadioService.IsStarted)
                {
                    notificationsService.Show("Стоп стоп стоп", "Вы не можете подключиться к радиостанции, потому что вы сами владелец радиостанции :)");
                    return;
                }


                var config = await configService.GetConfig();
                var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();

                if(listenTogetherService.IsConnectedToServer && listenTogetherService.PlayerMode == Core.Models.PlayerMode.Listener)
                {
                    notificationsService.Show("Стоп стоп стоп", "Ты уже подключен к серверу совместного прослушивания");

                    return;
                }

                if (listenTogetherService.IsConnectedToServer && listenTogetherService.PlayerMode == Core.Models.PlayerMode.Owner)
                {
                    notificationsService.Show("Стоп стоп стоп", "У тебя уже запущена сессия совместного прослушивания");
                }

                await listenTogetherService.ConnectToServerAsync(config.UserId);

                await listenTogetherService.JoinToSesstionAsync(station.SessionId);

            }catch(Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex);
                notificationsService.Show("Ошибка", "Мы не смогли подключиться к радиостанции :(");
            }
           
        }

        private async Task CreateStation()
        {
            var playerService = StaticService.Container.GetRequiredService<PlayerService>();
            var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            var userRadioService = StaticService.Container.GetRequiredService<UserRadioService>();

            if (!playerService.IsPlaying)
            {
                notificationsService.Show("Притормози-ка!", "Сначала запусти трек. Можешь запустить радиостанцию через меню совместного прослушивания :)");

                return;
            }

            if (userRadioService.IsStarted)
            {
                notificationsService.Show("Стоп стоп стоп", "У Вас уже запущена радиостанция. Зачем создавать ещё одну?");
                return;
            }

            navigationService.OpenModal<CreateUserRadioModal>(new CreateUserRadioModalViewModel());
        }
    }
}
