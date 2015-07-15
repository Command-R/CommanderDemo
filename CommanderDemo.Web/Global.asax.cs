using System;
using System.Data.Entity;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using CfgDotNet;
using CommanderDemo.Domain;
using CommandR;
using CommandR.Authentication;
using CommandR.MongoQueue;
using CommandR.Services;
using CommandR.WebApi;
using FluentScheduler;
using MediatR;
using SimpleInjector;
using SimpleInjector.Extensions;
using SimpleInjector.Extensions.LifetimeScoping;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using SimpleInjector.Integration.WebApi;

namespace CommanderDemo.Web
{
    public class Global : HttpApplication
    {
        private static Container _container;
        private static Assembly[] _assemblies;

        protected void Application_Start(object sender, EventArgs e)
        {
            _container = new Container();

            // Since we are using Mvc, WebApi, and FluentScheduler the LifetimeScope is needed
            // when an HttpContext.Current is not available.
            var lifestyle = Lifestyle.CreateHybrid(() => HttpContext.Current == null,
                new LifetimeScopeLifestyle(true),
                new WebRequestLifestyle(true));

            // These are all the assemblies that need to be scanned by the container.
            _assemblies = new[]
            {
                typeof(Global).Assembly,
                typeof(LoginUser).Assembly,
                typeof(Commander).Assembly,
                typeof(JsonRpcController).Assembly,
                typeof(MongoQueueService).Assembly,
            };

            ConfigureServices(lifestyle);
            ConfigureSettings();
            ConfigureMediator();
            ConfigureCommander(GlobalConfiguration.Configuration, lifestyle);
            ConfigureRoutes(GlobalConfiguration.Configuration, RouteTable.Routes);
            ConfigureMvc();
            ConfigureWebApi(GlobalConfiguration.Configuration);
            ConfigureFluentScheduler();

            _container.Verify();
        }

        //HACK: to simplify MVC + API reconsituting the TokenId from Forms Auth cookie-persisted Username
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (Context.Session == null)
                return;

            var tokenId = (string)Session["TokenId"];
            if (tokenId == null)
            {
                tokenId = _container.GetInstance<IMediator>().Send(new GetUserToken
                {
                    Username = User.Identity.Name
                });
                Session["TokenId"] = tokenId;
            }

            var tokenService = _container.GetInstance<ITokenService>();
            var dict = tokenService.GetTokenData(tokenId);
            _container.GetInstance<ExecutionEnvironment>().AppContext = new AppContext(dict)
            {
                RequestIsLocal = Request.IsLocal,
            };
        }

        private static void ConfigureServices(Lifestyle lifestyle)
        {
            _container.Register<ContactDb>(lifestyle);
            _container.Register<DbContext>(() => _container.GetInstance<ContactDb>()); //Used by TransactionHandler
            _container.Register<ITokenService, TokenService>();
            _container.RegisterSingle<IQueueService, MongoQueueService>();
            _container.Register<AuditService>(lifestyle);
        }

        private static void ConfigureSettings()
        {
            // Scan all our assemblies for ISettings, load their values from the supplied Providers,
            // and Validate so we can fail fast if anything is not set up correctly, then register
            // the settings in the container. Changes to the Settings classes will *not* be persisted, but
            // will be reloaded when the app is restarted.
            new SettingsManager()
                .AddProvider(new ConnectionStringsSettingsProvider())
                .AddProvider(new AppSettingsSettingsProvider())
                .AddSettings(_assemblies)
                .Validate()
                .ForEach(x => _container.RegisterSingle(x.GetType(), x));
        }

        private static void ConfigureMediator()
        {
            _container.RegisterSingle<IMediator>(() => new Mediator(_container.GetInstance, _container.GetAllInstances));

            _container.RegisterManyForOpenGeneric(typeof(IRequestHandler<,>), _assemblies);
            _container.RegisterManyForOpenGeneric(typeof(IAsyncRequestHandler<,>), _assemblies);
            _container.RegisterManyForOpenGeneric(typeof(INotificationHandler<>), _container.RegisterAll, _assemblies);
            _container.RegisterManyForOpenGeneric(typeof(IAsyncNotificationHandler<>), _container.RegisterAll, _assemblies);

            _container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionHandler<,>));
            _container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(LoggingHandler<,>));
            _container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(AuditHandler<,>));
            _container.RegisterDecorator(typeof(INotificationHandler<>), typeof(SignalrHandler<>));
        }

        private static void ConfigureCommander(HttpConfiguration config, Lifestyle lifestyle)
        {
            _container.Register<AppContext>(() => _container.GetInstance<ExecutionEnvironment>().AppContext ?? new AppContext(), lifestyle);
            _container.Register<ExecutionEnvironment>(lifestyle);

            _container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(AuthorizationHandler<,>));
            _container.RegisterDecorator(typeof(IAsyncRequestHandler<,>), typeof(AsyncAuthorizationHandler<,>));

            config.Filters.Add(new ApiAuthorizationFilter());
            Commander.Initialize(_assemblies); //Register all the commands
        }

        private static void ConfigureRoutes(HttpConfiguration config, RouteCollection routes)
        {
            config.MapHttpAttributeRoutes();

            routes.MapRoute("MvcControllers",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }

        private static void ConfigureMvc()
        {
            _container.RegisterMvcControllers(_assemblies);
            _container.RegisterMvcIntegratedFilterProvider();
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(_container));
        }

        private static void ConfigureWebApi(HttpConfiguration config)
        {
            _container.RegisterWebApiControllers(config, _assemblies);
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(_container);
            config.EnsureInitialized();
        }

        private static void ConfigureFluentScheduler()
        {
            TaskManager.Initialize(new TaskRegistry(_container));
        }
    };
}
