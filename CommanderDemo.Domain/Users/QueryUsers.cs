using System.Linq;
using CommandR;
using CommandR.Authentication;
using CommandR.Extensions;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Returns a list of Users.
    /// This shows an example of a command returning the original Query along with
    ///   the Result. This can make our MVC Controllers simpler since the razor view
    ///   can bind to the Query.Inactive property.
    /// CommandR provides the [Authorize] to secure the command
    /// CommandR provides IPageable and PagedList classes
    /// CommandR provides the IQuery (cqrs-ish) marker interface. Is only used by the
    /// api.tt example to auto-generate calls to commands for javascript.
    /// </summary>
    [Authorize]
    public class QueryUsers : IQuery, IPageable, IRequest<QueryUsers.Response>
    {
        public bool Inactive { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

        public class Response
        {
            public QueryUsers Query { get; set; }
            public PagedList<UserInfo> Result { get; set; }
        };

        public class UserInfo
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public bool IsActive { get; set; }
        };

        internal class Handler : IRequestHandler<QueryUsers, Response>
        {
            private readonly ContactDb _db;

            public Handler(ContactDb db)
            {
                _db = db;
            }

            public Response Handle(QueryUsers cmd)
            {
                var query = _db.Users.AsQueryable();

                if (!cmd.Inactive)
                {
                    query = query.Where(x => x.IsActive);
                }

                var result = query
                    .Select(x => new UserInfo
                    {
                        Id = x.Id,
                        Username = x.Username,
                        IsActive = x.IsActive,
                    })
                    .OrderBy(x => x.Id)
                    .ToPagedList(cmd, 25, 100);

                return new Response
                {
                    Query = cmd,
                    Result = result,
                };
            }
        };
    };
}
