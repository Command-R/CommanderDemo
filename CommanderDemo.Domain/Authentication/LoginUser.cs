using System;
using System.Collections.Generic;
using System.Linq;
using CommandR;
using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Creates a TokenId which can be used to retrieve the authenticated user's info.
    /// Tokens are normally used by javascript frameworks to authorize AJAX calls back
    /// to the server so the login only happens once.
    /// </summary>
    [AllowAnonymous]
    public class LoginUser : ICommand, IRequest<string>
    {
        public string Username { get; set; }
        public string Password { get; set; }

        internal class Handler : IRequestHandler<LoginUser, string>
        {
            private readonly ContactDb _db;
            private readonly ITokenService _tokenService;

            public Handler(ContactDb db, ITokenService tokenService)
            {
                _db = db;
                _tokenService = tokenService;
            }

            public string Handle(LoginUser cmd)
            {
                var user = _db.Users
                              .SingleOrDefault(x => x.Username == cmd.Username);

                if (user == null || user.Password != cmd.Password || !user.IsActive)
                    throw new ApplicationException("Invalid login");

                var tokenId = _tokenService.CreateToken(new Dictionary<string, object>
                {
                    {"Username", user.Username},
                    {"Roles", new string[0]},
                });

                return tokenId;
            }
        };
    };
}
