using System.Threading.Tasks;
using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Simple example that can be used to test that CommandR /jsonrpc endpoint works
    /// from something like postman and can execute IAsyncRequests.
    /// ex: POST http://localhost:64862/jsonrpc {"method":"AsyncPing","params":{"Name":"Postman"}}
    /// </summary>
    [AllowAnonymous]
    public class AsyncPing : IAsyncRequest<AsyncPing.Pong>
    {
        public string Name { get; set; }

        public class Pong
        {
            public string Message { get; set; }
        };

        internal class Handler : IAsyncRequestHandler<AsyncPing, Pong>
        {
            public Task<Pong> Handle(AsyncPing request)
            {
                var response = new Pong
                {
                    Message = "Hello (async): " + request.Name,
                };
                return Task.FromResult(response);
            }
        }
    };
}
