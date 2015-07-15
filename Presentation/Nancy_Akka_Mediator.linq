<Query Kind="Program">
  <NuGetReference>Akka</NuGetReference>
  <NuGetReference>Akka.DI.Core</NuGetReference>
  <NuGetReference>Nancy</NuGetReference>
  <NuGetReference>Nancy.Hosting.Self</NuGetReference>
  <NuGetReference>Nancy.Serialization.JsonNet</NuGetReference>
  <NuGetReference>SimpleInjector</NuGetReference>
  <Namespace>Akka</Namespace>
  <Namespace>Akka.Actor</Namespace>
  <Namespace>Akka.DI.Core</Namespace>
  <Namespace>CommandR</Namespace>
  <Namespace>MediatR</Namespace>
  <Namespace>Nancy</Namespace>
  <Namespace>Nancy.Extensions</Namespace>
  <Namespace>Nancy.Hosting.Self</Namespace>
  <Namespace>Nancy.ModelBinding</Namespace>
  <Namespace>Nancy.Serialization.JsonNet</Namespace>
  <Namespace>Nancy.TinyIoc</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>SimpleInjector</Namespace>
  <Namespace>SimpleInjector.Extensions</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

//Server
void Main() {
    using (var host = new NancyHost(new Uri("http://localhost:8080"), new LinqpadNancyBootstrapper())) {
        host.Start();
        Console.ReadLine();
    } 
}

//Nancy
public class HelloModule : NancyModule {
    private Mediator _mediator;
    public HelloModule() {
        var container = new Container();
        var system = ActorSystem.Create("JsonRpcActors");
        _mediator = new Mediator(container, system);
        container.RegisterSingle(_mediator);
        
        Get["/"] = _ => "POST to /jsonrpc";
        Post["/jsonrpc"] = JsonRpc;
    }
    private dynamic JsonRpc(dynamic args) {
        var request = JsonConvert.DeserializeObject<JsonRpcRequest>(Request.Body.AsString());
        var response = _mediator.SendAsync(request).Result;
        return response;
    }
};

//Command
public class Ping : IAsyncRequest<Ping.Pong> {
    public string Name { get; set; }
    public class Pong {
        public string Message { get; set; }
    };
    internal class Handler : TypedActor, IAsyncRequestHandler<Ping, Ping.Pong> {
		private TestService _service;
		public Handler(TestService service) {
			_service = service;
		}
        public Task<Ping.Pong> Handle(Ping cmd) {
            var response = new Pong {
                Message = _service.Message + cmd.Name,
            };
            Sender.Tell(response);
			return null;
        }
    };
};

//Service
internal class TestService {
	public string Message { get { return "Test: "; } }
};

//Akka Mediator
public class Mediator {
	private SimpleInjectorDependencyResolver _resolver;
	private ActorSystem _system;
	public Mediator(Container container, ActorSystem system) {
		_resolver = new SimpleInjectorDependencyResolver(container, system);
		_system = system;
	}
	public async Task<object> SendAsync(JsonRpcRequest request) {
        try {
            var cmdType = Type.GetType("UserQuery+" + request.method, true);
            var cmd = request.@params.ToObject(cmdType);
            var handler = Type.GetType(cmdType + "+Handler");
            var actor = _system.ActorOf(_resolver.Create(handler));
            var result = await actor.Ask(cmd);
            return new JsonRpcResponse {
                result = result
            };
        } catch (Exception ex) {
            return new JsonRpcResponse {
                error = new JsonRpcError {
                    message = ex.ToString()
                }
            };
        }
	}
};

//REF: http://getakka.net/docs/DI%20Core
public class SimpleInjectorDependencyResolver : IDependencyResolver {
    private Container _container;
    private ActorSystem _system;
    public SimpleInjectorDependencyResolver(Container container, ActorSystem system) {
        _container = container;
        _system = system;
        _system.AddDependencyResolver(this);
    }	
	public Type GetType(string actorName) {
        return null;
    }
    public Func<ActorBase> CreateActorFactory(Type actorType) {
        return () => (ActorBase)_container.GetInstance(actorType);
    }
    public Props Create<TActor>() where TActor : ActorBase {
        return Create(typeof(TActor));
    }	
	public Props Create(Type actorType) {
        return _system.GetExtension<DIExt>().Props(actorType);
    }
    public void Release(ActorBase actor) {
        //this.container.Release(actor);
    }
};

public class LinqpadNancyBootstrapper : DefaultNancyBootstrapper {
    protected override void ConfigureApplicationContainer(TinyIoCContainer container) {
    }
};