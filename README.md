# Command and Conquer

This is the demo application I showed off at the Charlotte Alt.net meeting on 7/15/2015. It explores how the command pattern and the Command-R projects can be used in an application. Some of my notes from the talk are below along with instructions on running the app. Feel free to open issues with any bugs or hit me up on twitter [@PaulWheeler](https://twitter.com/PaulWheeler). I'm thinking about creating another demo that does not contain the MVC part and is just an Aurelia.js SPA, and possibly an Angular.js version. Let me know if there is interest in either.

## Startup instructions

* Visual Studio will need to restore all the nuget packages

* You will need a .\SqlExpress Sql Server database, or configure a ConnectionString in the Web.config as below

```
<connectionStrings>
  <add name="ContactDb" connectionString="..." providerName="System.Data.SqlClient" />
</connectionStrings>
```

* Follow [these steps](https://github.com/aurelia/app-contacts) in the CommanderDemo.Web folder to get Aurelia working. This will create \jspm_packages\ and \node_modules\ folders. Don't forget to run gulp build after making any changes to the \src\*.js files. You can use the Visual Studio extension [Task Runner Explorer](https://visualstudiogallery.msdn.microsoft.com/8e1b4368-4afb-467a-bc13-9650572db708) to automatically run gulp after a VS build.

* If you want to turn on the MongoQueueService or AuditService you'll need to [download MongoDb](http://www.mongodb.org/downloads) then [install it](http://docs.mongodb.org/manual/tutorial/install-mongodb-on-windows/). You can create a "start.bat" file like below in the \mongodb\bin\ folder to start it up.

```
SET Db_Folder=.\db
IF NOT EXIST "%Db_Folder%" MKDIR "%Db_Folder%"
mongod.exe --dbpath "%Db_Folder%"
PAUSE
```

* You can enable the optional services by setting IsDisabled to "false" in the web.config (eg key="TaskRegistry+Settings.IsDisabled" value="false")

## Command-R Projects

* **Command-R** - Base Nuget package that can deserialize a JsonRpcRequest string (provided via any mechanism), and pass the IRequest or IAsyncRequest along to MediatR to be executed, then serialize back the JsonRpcResponse or JsonRpcError.

* **Command-R.WebApi** - Provides a JsonRpcController using WebAPI to provide a single /jsonrpc endpoint.

* **Command-R.MongoQueue** - A queue implemented using MongoDb tailable cursors (thanks Matt) that can be used to process tasks in the background or by services running on other servers.

* **CfgDotNet** - Allows you to store your application configuration data in cfg.json files, and supports multiple environments (eg local, dev, qa, uat, prod). Also contains SettingsManager which enables strongly-typed settings.

## What is the Command Pattern?

> In object-oriented programming, the command pattern is a behavioral design pattern in which an object is used to *encapsulate all information needed to perform an action* or trigger an event at a later time. This information includes the method name, the object that owns the method and values for the method parameters.

[https://en.wikipedia.org/?title=Command_pattern](https://en.wikipedia.org/?title=Command_pattern)

The focus of the Command-R project is to make it easy to structure applications around the command pattern. A simple example command might look like this:

```
public class Ping : IRequest<Ping.Pong>
{
    public string Name { get; set; }

    public class Pong 
    {
        public string Message { get; set; }
    }

    internal class Handler : IRequestHandler<Ping, Pong> 
    {     
        public Pong Handle(Ping request)
        {
            return new Pong { 
                Message = "Ping: " + request.Name
            };
        }
    }
}
```

Command-R allows us to execute these commands with [MediatR](https://github.com/jbogard/MediatR) via the [JSON-RPC 2.0 specification](http://www.jsonrpc.org/specification). Command-R has two minor extensions to the JSON-RPC spec. 1) You can upload files who paths on the server are mapped to matching property names on the command. 2) Added the IPatchable interface which sends along a list of which properties existed in the json params request dictionary that was sent.
## Mediator Pattern

> In Software Engineering, the mediator pattern defines an object that encapsulates how a set of objects interact. This pattern is considered to be a behavioral pattern due to the way it can alter the program's running behavior.

[https://en.wikipedia.org/wiki/Mediator_pattern](https://en.wikipedia.org/wiki/Mediator_pattern)

Jimmy Bogard's MediatR project uses .Net generics along with your choice of IoC container to execute requests, which have a single handler and return a response, and notifications which can have many handlers but no response.

```
//Request with string response
public class Ping : IRequest<string> {}

//Handler
public class PingHandler : IRequestHandler<Ping, string> {
    public string Handle(Ping request) {
        return "Pong";
    }
}
```

The one model in, one model out pattern greatly simplifies conceptualizing the system and realizing more powerful patterns. The single Handler interface method represents the ability to take an input model, perform work, and return an output model.

Besides the handlers for specific requests, you can define generic handlers will will be run for all requests. These should be registered explicitly in the container using open generics (eg map RequestLoggingHandler<,> to IRequestHandler<,>) since the order they are added matters (last one runs first).

```
public class RequestLoggingHandler<TReq, TResp> : IRequestHandler<TReq, TResp> where TReq:IRequest<TResp> 
{
    private readonly IRequestHandler<TReq, TResp> _inner;

    public RequestLoggingHandler(IRequestHandler<TReq, TResp> inner) {
        _inner = inner;
    }

    public TResp Handle(TReq request) {
        Console.WriteLine("RequestLoggingHandler (before): " + request.GetType().Name);
        var response = _inner.Handle(request);
        Console.WriteLine("RequestLoggingHandler (after): " + request.GetType().Name);
        return response;
    }
}

//container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(RequestLoggingHandler<,>));
```

Since the request handlers all come from the container, any dependencies will automatically be injected. This allows you to build sophisticated commands that need to access external resources (database, services, etc).

```
public class Ping : IRequest<Ping.Pong> 
{
    public string Name { get; set; }

    public class Pong {
        public string Message { get; set; }
    }

    internal class Handler : IRequestHandler<Ping, Pong> 
    {
        private readonly MessageService _messageService;

        public Handler(MessageService messageService) {
            _messageService = messageService;
        }

        public Pong Handle(Ping request) {
            return new Pong { 
                Message = _messageService.GetMessage() + "Ping: " + request.Name
            };
        }
    }
}

internal class MessageService 
{
    public string GetMessage() {
        return "MessageService: ";
    }
}

```

## Strongly-typed Settings

One of the new features of Asp.net 5 is that it allows you to define your own [strongly-typed AppSettings classes](https://weblog.west-wind.com/posts/2015/Jun/03/Strongly-typed-AppSettings-Configuration-in-ASPNET-5). These settings values can be loaded via various sources and then injected into the classes where needed via an IoC container.

The CfgDotNet project allows a similar mechanism with its SettingsManager class. The SettingsManager will scan all requsted assemblies for any ISettings present, load their values from the configured providers, then inject them into the IoC container.

It supports various sources for settings:
* AppSettings
* ConnectionStrings
* cfg.json
* Database
* Any other ISettingsProvider you want to write

In addition, the optional ISetting.Validate method allows each setting to verify it is configured correctly and any external resources (database, folders, etc) are available. Any exceptions thrown by a validation causes the application to fail fast on startup with clear diagnostic info.

```
const string environment = "prod";
const string cfgPath = "cfg.json";
var assemblies = new[] { GetType().Assembly };

new SettingsManager()
    .AddProvider(new ConnectionStringsSettingsProvider())
    .AddProvider(new AppSettingsSettingsProvider())
    .AddProvider(new CfgDotNetSettingsProvider(environment, cfgPath))
    .AddSettings(assemblies)
    .AddProvider<DbSettings>(x => new SqlDatabaseSettingsProvider(x.ConnectionString))
    .LoadSettingsFromProviders()
    .Validate()
    .ForEach(x => Container.RegisterSingle(x.GetType(), x));
```

## Additional Reading

* [MediatR Project](https://github.com/jbogard/MediatR)

* [LINQPad](https://www.linqpad.net/)

* [JSON-RPC 2.0 Specifiation](http://www.jsonrpc.org/specification)

* [Tackling cross-cutting concerns with a mediator pipeline](https://lostechies.com/jimmybogard/2014/09/09/tackling-cross-cutting-concerns-with-a-mediator-pipeline/)

* [Analyzing a DDD application](http://ayende.com/blog/153889/limit-your-abstractions-analyzing-a-ddd-application)

* [The key is in the infrastructure](http://ayende.com/blog/154241/limit-your-abstractions-the-key-is-in-the-infrastructure)

* [Put your controllers on a diet: POSTs and commands](https://lostechies.com/jimmybogard/2013/12/19/put-your-controllers-on-a-diet-posts-and-commands/)

* [CQRS with MediatR and AutoMapper](https://lostechies.com/jimmybogard/2015/05/05/cqrs-with-mediatr-and-automapper/)

* [Why the n-layer approach is bad for us all](http://tech.pro/blog/1498/why-the-n-layer-approach-is-bad-for-us-all)

* [DDD – Special scenarios, part 2](https://lostechies.com/gabrielschenker/2015/05/11/ddd-special-scenarios-part-2/)

* [DDD applied](https://lostechies.com/gabrielschenker/2015/04/28/ddd-applied/)

* [Strongly typed AppSettings Configuration in ASP.NET 5](https://weblog.west-wind.com/posts/2015/Jun/03/Strongly-typed-AppSettings-Configuration-in-ASPNET-5)

* [cmder Portable console emulator for Windows](http://gooseberrycreative.com/cmder/)

* [Task Runner Explorer](https://visualstudiogallery.msdn.microsoft.com/8e1b4368-4afb-467a-bc13-9650572db708)

* [mongoDB](https://www.mongodb.org/)

* [SimpleInjector IoC](https://simpleinjector.org/index.html)

* [Akka.net Actor framework](http://getakka.net/)

* [FakeItEasy TDD mocks/fakes/stubs](https://github.com/FakeItEasy/FakeItEasy)

* [Shouldly TDD assertion library](https://github.com/shouldly/shouldly)

* [xUnit.net TDD](http://xunit.github.io/)

* [FluentScheduler background task runner](https://github.com/jgeurts/FluentScheduler)

* [JWT JSON Web Token](https://github.com/jwt-dotnet/jwt)
