using System;
using System.Diagnostics;
using CfgDotNet;
using CommanderDemo.Domain;
using CommandR;
using CommandR.Authentication;
using CommandR.Services;
using FluentScheduler;
using FluentScheduler.Model;
using SimpleInjector;

namespace CommanderDemo.Web
{
    /// <summary>
    /// Configure the schedule for the background tasks we want FluentScheduler to run
    /// for us in the background.
    /// </summary>
    public class TaskRegistry : Registry
    {
        private readonly Container _container;
        private readonly Commander _commander;
        private readonly AppContext _system;

        public TaskRegistry(Container container)
        {
            _container = container;
            _commander = _container.GetInstance<Commander>();

            var settings = _container.GetInstance<Settings>();
            if (settings.IsDisabled)
            {
                TaskManager.Stop();
                return;
            }

            TaskManager.UnobservedTaskException += TaskManager_UnobservedTaskException;

            //We'll run our system tasks as Admin
            _system = new AppContext { Username = "Admin" };

            Schedule(StartQueueInfiniteLoop).ToRunOnceIn(5).Seconds();
            Schedule(PingMe).ToRunNow().AndEvery(5).Seconds();
        }

        /// <summary>
        /// The MongoQueueService uses tailable cursors which block until a new item
        /// is added. Since all tasks run in the background, no problem.
        /// </summary>
        private void StartQueueInfiniteLoop()
        {
            var cancellation = new System.Threading.CancellationTokenSource();
            var queueService = _container.GetInstance<IQueueService>();
            queueService.StartProcessing(cancellation.Token, Send);
        }

        /// <summary>
        /// Just an example, serves no point, but you should see it in the Output window due
        /// to the LoggingHandler.
        /// </summary>
        private void PingMe()
        {
            Send(new Ping {Name = "Schedule"}, _system);
        }

        /// <summary>
        /// Since each task execution runs in an background thread, set up container lifetime and provide context.
        /// </summary>
        private void Send(object command, AppContext context)
        {
            using (_container.BeginLifetimeScope())
            {
                _container.GetInstance<ExecutionEnvironment>().AppContext = context;
                _commander.Send(command).Wait();
            }
        }

        /// <summary>
        /// Any exceptions that occur in the background task threads are caught here. We should probably log.
        /// </summary>
        private static void TaskManager_UnobservedTaskException(TaskExceptionInformation sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("TASK ERROR: " + e.ExceptionObject);
        }

        internal class Settings : BaseSettings
        {
            //inherits IsDisabled
        };
    };
}
