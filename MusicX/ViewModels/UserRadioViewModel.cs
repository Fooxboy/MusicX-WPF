using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Shared.ListenTogether.Radio;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.ViewModels
{
    public class UserRadioViewModel : BaseViewModel
    {
        public ObservableCollection<Station> Developers { get; set; }

        public ObservableCollection<Station> Recommended { get; set; }

        public ObservableCollection<Station> UserStations { get; set; }

        public UserRadioViewModel()
        {
            Developers = new ObservableCollection<Station>();
            Recommended = new ObservableCollection<Station>();
            UserStations = new ObservableCollection<Station>();

            //Developers.Add(new Station()
            //{
            //    Cover = "https://as2.ftcdn.net/v2/jpg/02/87/95/77/1000_F_287957705_kVUIWM8TnTbavhGX9JTEAQLGQo6fVrc5.jpg",
            //    Description = "Я просто запустил плейлист ТОП100 из рекомендаций ВКонтакте. Я хочу чтобы не только у меня пошла кровь из ушей :)",
            //    Owner = new StationOwner() { Name = "Славик Смирнов", VkId = 12432423 },
            //    SessionId = "sdkf34834nfjs",
            //    Title = "Кровь из ушей FM?"
            //});


            //UserStations.Add(new Station()
            //{
            //    Cover = "https://cdn.popcake.tv/wp-content/uploads/2021/02/instasamka_143538967_114554623901527_3870684183870930796_n.jpg",
            //    Description = "СЛУШАЕМ ИНСТАСАМКУ ДО ПОТЕРИ ПАМЯТИ!!!!!!!!!!!!",
            //    Owner = new StationOwner() { Name = "Иван Иванов", VkId = 12432423 },
            //    SessionId = "sdkf34834nfjs",
            //    Title = "Инстасасамка"
            //});

            //UserStations.Add(new Station()
            //{
            //    Cover = "https://www.dhresource.com/webp/m/0x0/f2/albu/g19/M01/72/85/rBVap2BZstmAWe7LAALkGyrdSkE013.jpg",
            //    Description = "Вспомним былое, самые крутые треки 2000х",
            //    Owner = new StationOwner() { Name = "Андрей Миллениалов", VkId = 12432423 },
            //    SessionId = "sdkf34834nfjs",
            //    Title = "2000х"
            //});

            //UserStations.Add(new Station()
            //{
            //    Cover = "https://img.championat.com/s/735x490/news/big/u/o/nad-anime-serialom-po-genshin-impact-rabotayut-avtory-hita-istrebitel-demonov_16633379752078206607.jpg",
            //    Description = "Создал плейлист на 5000 треков из самых популярных аниме, го слушать",
            //    Owner = new StationOwner() { Name = "Кадзуто Киригая", VkId = 12432423 },
            //    SessionId = "sdkf34834nfjs",
            //    Title = "Аниме радио"
            //});
        }


        public async Task LoadData()
        {
            var radioService = StaticService.Container.GetRequiredService<UserRadioService>();

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
    }
}
