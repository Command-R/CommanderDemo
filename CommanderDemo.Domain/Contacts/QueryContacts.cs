using System.Linq;
using CommandR;
using CommandR.Authentication;
using CommandR.Extensions;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Returns a list of Contacts that match the Search criteria.
    /// CommandR provides IQuery (cqrs-like) marker interface.
    /// CommandR provides IPageable and PagedList classes.
    /// </summary>
    [Authorize]
    public class QueryContacts : IQuery, IPageable, IRequest<PagedList<QueryContacts.ContactInfo>>
    {
        public string Search { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

        public class ContactInfo
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        };

        internal class Handler : IRequestHandler<QueryContacts, PagedList<ContactInfo>>
        {
            private readonly ContactDb _db;

            public Handler(ContactDb db)
            {
                _db = db;
            }

            public PagedList<ContactInfo> Handle(QueryContacts cmd)
            {
                var query = _db.Contacts.AsQueryable();

                if (!string.IsNullOrEmpty(cmd.Search))
                {
                    int id;
                    int.TryParse(cmd.Search, out id);
                    query = query.Where(x => x.Id == id
                                             || x.FirstName.Contains(cmd.Search)
                                             || x.LastName.Contains(cmd.Search)
                                             || x.Email.Contains(cmd.Search)
                                             || x.PhoneNumber == cmd.Search);
                }

                return query
                    .Select(x => new ContactInfo
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                    })
                    .OrderBy(x => x.Id)
                    .ToPagedList(cmd, 25, 100);
            }
        };
    };
}
