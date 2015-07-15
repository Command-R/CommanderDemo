using CommandR;
using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Deletes a Contact.
    /// CommandR provides ICommand (cqrs-like) marker interface (only used by api.tt)
    /// </summary>
    [Authorize]
    public class DeleteContact : ICommand, IRequest<bool>
    {
        public int Id { get; set; }

        internal class Handler : IRequestHandler<DeleteContact, bool>
        {
            private readonly ContactDb _db;

            public Handler(ContactDb db)
            {
                _db = db;
            }

            public bool Handle(DeleteContact cmd)
            {
                var contact = _db.Contacts.Find(cmd.Id);
                _db.Contacts.Remove(contact);
                _db.SaveChanges();
                return true;
            }
        };
    };
}
