using System.Diagnostics;
using MediatR;
using Newtonsoft.Json;

namespace CommanderDemo.Web
{
    /// <summary>
    /// Simple handler that just prints the command and response to the Output window.
    /// </summary>
    public class LoggingHandler<TReq, TResp> : IRequestHandler<TReq, TResp> where TReq : IRequest<TResp>
    {
        private readonly IRequestHandler<TReq, TResp> _inner;

        public LoggingHandler(IRequestHandler<TReq, TResp> inner)
        {
            _inner = inner;
        }

        public TResp Handle(TReq request)
        {
            Debug.WriteLine("{0} (Request) ===================================\r\n{1}",
                request.GetType().Name,
                JsonConvert.SerializeObject(request, Formatting.Indented));

            var response = _inner.Handle(request);

            Debug.WriteLine("{0} (Response) ===================================\r\n{1}",
                request.GetType().Name,
                JsonConvert.SerializeObject(response, Formatting.Indented));

            return response;
        }
    };
}