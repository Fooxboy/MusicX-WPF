using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Shared.ListenTogether.Radio;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserStationControl.xaml
    /// </summary>
    public partial class UserStationControl : UserControl
    {
        public Station Station
        {
            get => (Station)GetValue(StationProperty);
            set => SetValue(StationProperty, value);
        }
        public static readonly DependencyProperty StationProperty = DependencyProperty.Register(nameof(Station), typeof(Station), typeof(UserStationControl));

        public UserStationControl()
        {
            this.Loaded += UserStationControl_Loaded;
            InitializeComponent();
        }

        private void UserStationControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title.Text = Station.Title;
            this.Author.Text = Station.Owner.Name;
            this.Description.Text = Station.Description;
            this.Cover.ImageSource = new BitmapImage(new Uri(Station.Cover));
        }

        private void Card_MouseEnter(object sender, MouseEventArgs e)
        {
            RadioCard.Opacity = 0.7;
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            RadioCard.Opacity = 1;
        }

        private async void RadioCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listenTogetherService = StaticService.Container.GetRequiredService<ListenTogetherService>();
            var userRadioService = StaticService.Container.GetRequiredService<UserRadioService>();
            var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

            if(userRadioService.IsStarted)
            {
                notificationsService.Show("Стоп стоп стоп", "Вы не можете подключиться к радиостанции, потому что вы сами владелец радиостанции :)");
                return;
            }

            try
            {
                await listenTogetherService.JoinToSesstionAsync(Station.SessionId);

            }catch(Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();
                logger.Error(ex);

                notificationsService.Show("Ошибка", "Мы не смогли подключиться к радиостанции");
            }
        }
    }
}
