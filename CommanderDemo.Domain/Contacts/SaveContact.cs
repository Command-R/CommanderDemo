using System;
using System.Linq;
using CommandR;
using CommandR.Authentication;
using CommandR.Extensions;
using CommandR.Services;
using MediatR;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// Retrieve a User by id, or create a new unsaved one for binding.
    /// CommandR provides CopyTo method. Similar to AutoMapper, but supports
    ///   CommandR's extension to JsonRpc, IPatchable which automatically maps
    ///   which properties were actually included by the caller (eg only Username).
    /// </summary>
    [Authorize]
    public class SaveContact : ICommand, IPatchable, IRequest<int>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string[] PatchFields { get; set; }

        internal class Handler : IRequestHandler<SaveContact, int>
        {
            private readonly ContactDb _db;
            private readonly AppContext _appContext;
            private readonly IQueueService _queue;
            private readonly IMediator _mediator;

            public Handler(ContactDb db, AppContext appContext, IQueueService queue, IMediator mediator)
            {
                _db = db;
                _appContext = appContext;
                _queue = queue;
                _mediator = mediator;
            }

            public int Handle(SaveContact cmd)
            {
                if (string.IsNullOrWhiteSpace(cmd.Email))
                    throw new ApplicationException("Invalid email: " + cmd.Email);

                if (string.IsNullOrWhiteSpace(cmd.PhoneNumber))
                    throw new ApplicationException("Invalid phone: " + cmd.PhoneNumber);

                var contact = _db.Contacts.Find(cmd.Id)
                              ?? new Contact();

                cmd.CopyTo(contact, cmd.PatchFields);
                if (contact.Id == 0) _db.Contacts.Add(contact);
                _db.SaveChanges();

                var contactInfo = contact.GetType().GetProperties().Select(x => x.Name + ": " + x.GetValue(contact));
                _queue.Enqueue(new SendEmail
                {
                    To = "paul@paulwheeler.com",
                    Subject = "Contact Saved",
                    Body = string.Join(Environment.NewLine, contactInfo),
                }, _appContext);

                _mediator.Publish(new Alert { Message = "Contact Saved: " + contact.Id });

                return contact.Id;
            }
        };
    };
}
