using System.Collections.Generic;
using System.Linq;
using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// HACK: Internal command to create a token from the already-validated Asp.net Identity since
    /// we are mixing MVC and API tokens
    /// </summary>
    [AllowAnonymous]
    internal class GetUserToken : IRequest<string>
    {
        public string Username { get; set; }

        internal class Handler : IRequestHandler<GetUserToken, string>
        {
            private readonly ContactDb _db;
            private readonly ITokenService _tokenService;

            public Handler(ContactDb db, ITokenService tokenService)
            {
                _db = db;
                _tokenService = tokenService;
            }

            public string Handle(GetUserToken cmd)
            {
                var user = _db.Users
                              .SingleOrDefault(x => x.Username == cmd.Username);

                if (user == null)
                    return null;

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
