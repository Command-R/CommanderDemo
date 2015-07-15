using System;
using System.Collections.Generic;
using System.Linq;
using CommandR.Authentication;
using MediatR;

//_container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(AuditHandler<,>));
namespace CommanderDemo.Web
{
    /// <summary>
    /// The AuditHandler uses the AuditService to persist all commands and responses to Mongo.
    /// This functionality will eventually be cleaned up and added as a separate Nuget package.
    /// </summary>
    internal class AuditHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _inner;
        private readonly AuditService _auditService;
        private readonly ExecutionEnvironment _executionEnvironment;
        private readonly AuditService.Settings _settings;
        private readonly List<string> _inclusionList;
        private readonly List<string> _exclusionList;

        public AuditHandler(IRequestHandler<TRequest, TResponse> inner, AuditService auditService, ExecutionEnvironment executionEnvironment, AuditService.Settings settings)
        {
            _inner = inner;
            _auditService = auditService;
            _executionEnvironment = executionEnvironment;
            _settings = settings;

            _inclusionList = (settings.IncludeCommands ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            _exclusionList = (settings.ExcludeCommands ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public TResponse Handle(TRequest request)
        {
            if (_settings.IsDisabled)
                return _inner.Handle(request);

            var requestName = request.GetType().Name;
            var context = _executionEnvironment.AppContext;

            if ((_inclusionList.Count > 0 && !_inclusionList.Contains(requestName)) || _exclusionList.Contains(requestName))
                return _inner.Handle(request);

            try
            {
                _auditService.AddChild(new AuditDocument("Request", requestName, request), context);
                var response = _inner.Handle(request);
                _auditService.AddChild(new AuditDocument("Response", typeof(TResponse).FullName, response), context);
                return response;
            }
            catch (Exception ex)
            {
                var exceptionInfo = new ExceptionInfo(ex);
                _auditService.AddChild(new AuditDocument("ExceptionInfo", exceptionInfo.GetType().FullName, exceptionInfo), context);
                throw;
            }
        }
    };

    [Serializable]
    public class ExceptionInfo
    {
        public ExceptionInfo(Exception exception)
        {
            Source = exception.Source;
            Message = exception.Message;
            StackTrace = exception.StackTrace;
        }

        public string Source { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    };
}
