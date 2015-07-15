using CommandR;
using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// CommandR provides [AllowAnonymous] to indicate authentication isn't
    /// required for this command. The ApiAuthorizationFilter will throw
    /// an exception if a command doesn't have Authorize or AllowAnonymous attributes.
    /// </summary>
    [AllowAnonymous]
    public class LogoutUser : ICommand, IRequest
    {
        public string TokenId { get; set; }

        internal class Handler : RequestHandler<LogoutUser>
        {
            private readonly ITokenService _tokenService;

            public Handler(ITokenService tokenService)
            {
                _tokenService = tokenService;
            }

            protected override void HandleCore(LogoutUser cmd)
            {
                _tokenService.DeleteToken(cmd.TokenId);
            }
        };
    };
}
