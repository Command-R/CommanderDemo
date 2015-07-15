using CfgDotNet;
using Microsoft.AspNet.SignalR;

namespace CommanderDemo.Web
{
    /// <summary>
    /// This Hub is used the SignalrHandler to send INotifications to the clients.
    /// It might not be necessary but I'm new to SignalR and was just matching examples
    /// I saw online.
    /// </summary>
    public class NotificationHub : Hub
    {
        internal class Settings : BaseSettings
        {
            //inherits IsDisabled
        };
    };
}
