using System.Net.Mail;
using System.Threading.Tasks;
using CommandR;
using CommandR.Authentication;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// This command can be added to and IQueueService and be executed in the background
    /// by FluentScheduler.
    /// </summary>
    [Authorize]
    public class SendEmail : ITask, IAsyncRequest<Unit>
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        internal class Handler : IAsyncRequestHandler<SendEmail, Unit>
        {
            public async Task<Unit> Handle(SendEmail sendEmail)
            {
                var email = new MailMessage();
                email.To.Add(sendEmail.To);
                email.Subject = sendEmail.Subject;
                email.Body = sendEmail.Body;

                var smtp = new SmtpClient();
                await smtp.SendMailAsync(email);

                return Unit.Value;
            }
        };
    };
}
