using System.Web.Mvc;
using Owin;

namespace CommanderDemo.Web
{
    /// <summary>
    /// Bootstrap SignalR
    /// </summary>
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var hubSettings = DependencyResolver.Current.GetService<NotificationHub.Settings>();
            if (hubSettings.IsDisabled)
                return;

            app.MapSignalR();
        }
    };
}
