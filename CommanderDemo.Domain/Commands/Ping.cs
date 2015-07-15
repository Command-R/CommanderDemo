using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Simple example that can be used to test that CommandR /jsonrpc endpoint works
    /// from something like postman and can execute IRequests.
    /// ex: POST http://localhost:64862/jsonrpc {"method":"Ping","params":{"Name":"Postman"}}
    /// </summary>
    [AllowAnonymous]
    public class Ping : IRequest<Ping.Pong>
    {
        public string Name { get; set; }

        public class Pong
        {
            public string Message { get; set; }
        };

        internal class Handler : IRequestHandler<Ping, Pong>
        {
            public Pong Handle(Ping request)
            {
                return new Pong
                {
                    Message = "Hello (not async): " + request.Name,
                };
            }
        }
    };
}
