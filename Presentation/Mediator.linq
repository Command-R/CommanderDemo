<Query Kind="Program">
  <NuGetReference Version="2.0.0-beta-003" Prerelease="true">MediatR</NuGetReference>
  <NuGetReference>SimpleInjector</NuGetReference>
  <Namespace>MediatR</Namespace>
  <Namespace>SimpleInjector</Namespace>
  <Namespace>SimpleInjector.Extensions</Namespace>
</Query>

void Main() {
    var mediator = CreateMediator();
    var response = mediator.Send(new Ping { Name = "Paul" });
    Console.WriteLine(response.Message);
    mediator.Publish(new OrderCreated { Id=1 });
}

public class Ping : IRequest<Ping.Pong> 
{
    public string Name { get; set; }
    
    public class Pong 
    {
        public string Message { get; set; }
    }
    
    internal class PingHandler : IRequestHandler<Ping, Pong> {
        private readonly MessageService _messageService;
        
        public PingHandler(MessageService messageService) {
            _messageService = messageService;
        }
        
        public Pong Handle(Ping request) {
            return new Pong { 
                Message = "Pong: " + _messageService.GetMessage() + request.Name
            };
        }
    }
}

public class OrderCreated : INotification {
    public int Id { get; set; }
}

internal class OrderCreatedHandler1 : INotificationHandler<OrderCreated> {
    public void Handle(OrderCreated message) {
        Console.WriteLine("OrderCreatedHandler1: Handled Order #" + message.Id);
    }
}

internal class OrderCreatedHandler2 : INotificationHandler<OrderCreated> {
    public void Handle(OrderCreated message) {
        Console.WriteLine("OrderCreatedHandler2: Handled Order #" + message.Id);
    }
}

internal class MessageService {
    public string GetMessage() {
        return "MessageService: Hello ";
    }
}

internal class RequestLoggingHandler<TReq, TResp> : IRequestHandler<TReq, TResp> where TReq:IRequest<TResp> {
    private readonly IRequestHandler<TReq, TResp> _inner;
    public RequestLoggingHandler(IRequestHandler<TReq, TResp> inner) {
        _inner = inner;
    }
    public TResp Handle(TReq request) {
        Console.WriteLine("RequestLoggingHandler: " + request.GetType().Name);
        return _inner.Handle(request);
    }
}

internal class NotificationLoggingHandler<T> : INotificationHandler<T> where T:INotification {
    private readonly INotificationHandler<T> _inner;
    public NotificationLoggingHandler(INotificationHandler<T> inner) {
        _inner = inner;
    }
    public void Handle(T msg) {
        Console.WriteLine("NotificationLoggingHandler: " + msg.GetType());
        _inner.Handle(msg);
    }
}

static IMediator CreateMediator() {
    var assemblies = new[] { Assembly.GetExecutingAssembly() };
    var container = new Container();
    container.RegisterSingle<IMediator>(() => new Mediator(container.GetInstance, container.GetAllInstances));
    
    container.RegisterManyForOpenGeneric(typeof(IRequestHandler<,>), assemblies);
    container.RegisterManyForOpenGeneric(typeof(IAsyncRequestHandler<,>), assemblies);
    container.RegisterManyForOpenGeneric(typeof(INotificationHandler<>), container.RegisterAll, assemblies);
    container.RegisterManyForOpenGeneric(typeof(IAsyncNotificationHandler<>), container.RegisterAll, assemblies);
    
    container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(RequestLoggingHandler<,>));
    container.RegisterDecorator(typeof(INotificationHandler<>), typeof(NotificationLoggingHandler<>));
    return container.GetInstance<IMediator>();
}