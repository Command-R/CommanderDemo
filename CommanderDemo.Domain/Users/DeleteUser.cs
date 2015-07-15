using System;
using CommandR;
using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Deletes a Contact.
    /// CommandR provides ICommand (cqrs-like) marker interface (only used by api.tt)
    /// </summary>
    [Authorize(Users = "Admin")]
    public class DeleteUser : ICommand, IRequest<bool>
    {
        public int Id { get; set; }

        internal class Handler : IRequestHandler<DeleteUser, bool>
        {
            private readonly ContactDb _db;

            public Handler(ContactDb db)
            {
                _db = db;
            }

            public bool Handle(DeleteUser cmd)
            {
                try
                {
                    var user = _db.Users.Find(cmd.Id);
                    _db.Users.Remove(user);
                    _db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("DeleteUser ERROR for Id: " + cmd.Id, ex);
                }
            }
        };
    };
}
