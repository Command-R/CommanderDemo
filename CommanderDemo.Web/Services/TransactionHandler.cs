using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using MediatR;

namespace CommanderDemo.Web
{
    /// <summary>
    /// Wraps inner handlers in an EntityFramework DbContextTransaction. This is not really used
    /// by our comands since most of them call SaveChanges directly, but is an example and technically
    /// makes their calls unnecessary (except the Saves that return new ids).
    /// </summary>
    internal class TransactionHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _inner;
        private readonly DbContext _db;

        public TransactionHandler(IRequestHandler<TRequest, TResponse> inner, DbContext db)
        {
            _inner = inner;
            _db = db;
        }

        public TResponse Handle(TRequest request)
        {
            if (_db.Database.CurrentTransaction != null)
                return _inner.Handle(request);

            using (var scope = _db.Database.BeginTransaction())
            {
                try
                {
                    var response = _inner.Handle(request);
                    _db.SaveChanges();
                    scope.Commit();
                    return response;
                }
                catch (DbEntityValidationException ex)
                {
                    scope.Rollback();
                    var errors = ex.EntityValidationErrors
                                   .SelectMany(x => x.ValidationErrors.Select(y => x.Entry.Entity.GetType().Name + ": " + y.ErrorMessage));
                    throw new ApplicationException(string.Join(Environment.NewLine, errors));
                }
            }
        }
    };
}
