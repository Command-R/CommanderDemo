using System;
using CommandR;
using CommandR.Authentication;
using CommandR.Extensions;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Retrieve a User by id, or create a new unsaved one for binding.
    /// CommandR provides ICommand (cqrs-like) marker interface (only used by api.tt)
    /// CommandR provides CopyTo method. Similar to AutoMapper, but supports
    ///   CommandR's extension to JsonRpc, IPatchable which automatically maps
    ///   which properties were actually included by the caller (eg only Username).
    /// </summary>
    [Authorize]
    public class SaveUser : ICommand, IPatchable, IRequest<int>
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public string[] PatchFields { get; set; }

        internal class Handler : IRequestHandler<SaveUser, int>
        {
            private readonly ContactDb _db;

            public Handler(ContactDb db)
            {
                _db = db;
            }

            public int Handle(SaveUser cmd)
            {
                if (string.IsNullOrEmpty(cmd.Username))
                    throw new ApplicationException("Username is required");

                if (string.IsNullOrEmpty(cmd.Password))
                    throw new ApplicationException("Password is required");

                var user = _db.Users.Find(cmd.Id)
                       ?? new User();

                cmd.CopyTo(user, cmd.PatchFields);
                if (user.Id == 0) _db.Users.Add(user);
                _db.SaveChanges();

                return user.Id;
            }
        };
    };
}
