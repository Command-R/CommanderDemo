using System;
using CommanderDemo.Domain;
using CommandR.Authentication;
using CommandR.Services;
using FakeItEasy;
using MediatR;
using Shouldly;
using Xunit;

namespace CommanderDemo.Test
{
    /// <summary>
    /// Test the SaveContact command.
    /// We have a shared _handler, but some tests create their own
    /// instance to mock the dependencies differently.
    /// </summary>
    public class TestSaveContact
    {
        private readonly ContactDb _db;
        private readonly AppContext _appContext;
        private readonly IQueueService _queue;
        private readonly IMediator _mediator;
        private readonly SaveContact.Handler _handler;

        public TestSaveContact()
        {
            var contacts = new[]
            {
                new Contact
                {
                    Id = 1,
                    FirstName = "First",
                    LastName = "Last",
                    Email = "test@example.com",
                    PhoneNumber = "555-1212"
                },
            }.ToDbSet();

            _db = A.Fake<ContactDb>();
            A.CallTo(() => _db.Contacts).Returns(contacts);

            _appContext = A.Fake<AppContext>();
            _queue = A.Fake<IQueueService>();
            _mediator = A.Fake<IMediator>();

            _handler = new SaveContact.Handler(_db, _appContext, _queue, _mediator);
        }

        [Fact]
        public void Test_SaveContact_Requires_Email()
        {
            var request = new SaveContact
            {
                FirstName = "First",
                LastName = "Last",
                Email = null,
                PhoneNumber = "555-1212",
            };
            Should.Throw<ApplicationException>(() => _handler.Handle(request));
        }

        [Fact]
        public void Test_SaveContact_Requires_Phone()
        {
            var request = new SaveContact
            {
                FirstName = "First",
                LastName = "Last",
                Email = "test@example.com",
                PhoneNumber = null,
            };
            Should.Throw<ApplicationException>(() => _handler.Handle(request));
        }

        [Fact]
        public void Test_SaveContact_Creates_New_Contact()
        {
            var request = new SaveContact
            {
                FirstName = "First",
                LastName = "Last",
                Email = "test@example.com",
                PhoneNumber = "555-1212",
            };
            var response = _handler.Handle(request);
            response.ShouldBe(2);
        }

        [Fact]
        public void Test_SaveContact_Enqueues_Email()
        {
            var request = new SaveContact
            {
                Id = 1,
                FirstName = "First",
                LastName = "Last",
                Email = "test@example.com",
                PhoneNumber = "555-1212",
            };

            var queue = A.Fake<IQueueService>();
            var handler = new SaveContact.Handler(_db, _appContext, queue, _mediator);
            handler.Handle(request);

            A.CallTo(() => queue.Enqueue(A<SendEmail>._, _appContext)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Test_SaveContact_Publishes_Alert()
        {
            var request = new SaveContact
            {
                Id = 1,
                FirstName = "First",
                LastName = "Last",
                Email = "test@example.com",
                PhoneNumber = "555-1212",
            };

            var mediator = A.Fake<IMediator>();
            var handler = new SaveContact.Handler(_db, _appContext, _queue, mediator);
            handler.Handle(request);

            A.CallTo(() => mediator.Publish(A<Alert>._)).MustHaveHappened(Repeated.Exactly.Once);
        }
    };
}
