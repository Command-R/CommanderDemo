using CommandR;
using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Retrieves a Contact by Id or returns a new unsaved object for binding.
    /// CommandR provides ICommand (cqrs-like) marker interface (only used by api.tt)
    /// </summary>
    [Authorize]
    public class GetContact : IQuery, IRequest<Contact>
    {
        public int Id { get; set; }

        internal class Handler : IRequestHandler<GetContact, Contact>
        {
            private readonly ContactDb _db;

            public Handler(ContactDb db)
            {
                _db = db;
            }

            public Contact Handle(GetContact cmd)
            {
                return _db.Contacts.Find(cmd.Id) ?? new Contact();
            }
        };
    };
}
