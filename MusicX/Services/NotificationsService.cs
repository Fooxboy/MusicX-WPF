using NLog;
using System.Windows;

namespace MusicX.Services
{
    public class NotificationsService
    {
        private readonly Logger logger;

        public event NotificationDelegate NewNotificationEvent;
        public delegate void NotificationDelegate(string title, string message);

        public NotificationsService(Logger logger)
        {
            this.logger = logger;
        }


        public void Show(string title, string message)
        {
            Application.Current.Dispatcher.BeginInvoke(()=>
            {
                NewNotificationEvent?.Invoke(title, message);

            });
        }
    }
}
