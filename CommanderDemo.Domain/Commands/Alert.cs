using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// The Alert notification allows us to send messages to the client
    /// via SignalR. Just a silly example of potential ways to use
    /// MediatR INotification.
    /// </summary>
    public class Alert : INotification
    {
        public string Message { get; set; }

        internal class Handler : INotificationHandler<Alert>
        {
            public void Handle(Alert notification)
            {
                //HACK: nothing to do here, the SignalrHandler does the work.
            }
        };
    };
}
