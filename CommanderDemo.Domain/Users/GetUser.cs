using CommandR;
using CommandR.Authentication;
using CommandR.Extensions;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Retrieve a User by id, or create a new unsaved one for binding.
    /// CommandR provides CopyTo method (similar to AutoMapper).
    /// </summary>
    [Authorize]
    public class GetUser : IQuery, IPatchable, IRequest<GetUser.UserInfo>
    {
        public int Id { get; set; }
        public string[] PatchFields { get; set; }

        public class UserInfo
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool IsActive { get; set; }
        };

        internal class Handler : IRequestHandler<GetUser, UserInfo>
        {
            private readonly ContactDb _db;

            public Handler(ContactDb db)
            {
                _db = db;
            }

            public UserInfo Handle(GetUser cmd)
            {
                var user = _db.Users.Find(cmd.Id)
                           ?? new User();

                return user.CopyTo(new UserInfo());
            }
        };
    };
}
