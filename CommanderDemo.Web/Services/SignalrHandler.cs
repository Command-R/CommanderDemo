using MediatR;
using Microsoft.AspNet.SignalR;

namespace CommanderDemo.Web
{
    /// <summary>
    /// Sends all MediatR INotifications to SignalR clients. Just a silly example
    /// of how you could use MediatR INotifications.
    /// </summary>
    public class SignalrHandler<T> : INotificationHandler<T> where T : INotification
    {
        private readonly INotificationHandler<T> _inner;

        public SignalrHandler(INotificationHandler<T> inner)
        {
            _inner = inner;
        }

        public void Handle(T notification)
        {
            _inner.Handle(notification);

            //Send to clients
            var hub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hub.Clients.All.publish(notification);
        }
    };
}
