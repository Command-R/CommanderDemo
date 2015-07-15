using System.Linq;
using CommanderDemo.Domain;
using FakeItEasy;
using Shouldly;
using Xunit;

namespace CommanderDemo.Test
{
    /// <summary>
    /// Test the QueryContacts command.
    /// We can use a single _handler instance since our command has only
    /// one method, we're just changing the inputs and testing the outputs.
    /// </summary>
    public class TestQueryContacts
    {
        private readonly QueryContacts.Handler _handler;

        public TestQueryContacts()
        {
            //Our Fake Contact data for the DbContext
            var contacts = new[]
            {
                new Contact {Id = 1, FirstName = "A", LastName = "B", Email = "C", PhoneNumber = "6"},
                new Contact {Id = 2, FirstName = "D", LastName = "E", Email = "F", PhoneNumber = "7"},
                new Contact {Id = 3, FirstName = "G", LastName = "H", Email = "I", PhoneNumber = "8"},
                new Contact {Id = 4, FirstName = "J", LastName = "K", Email = "L", PhoneNumber = "9"},
                new Contact {Id = 5, FirstName = "M", LastName = "N", Email = "O", PhoneNumber = "0"}
            }.ToDbSet();

            var db = A.Fake<ContactDb>();
            A.CallTo(() => db.Contacts).Returns(contacts);

            _handler = new QueryContacts.Handler(db);
        }

        [Fact]
        public void Test_QueryContacts_Returns_All_By_Default()
        {
            var request = new QueryContacts();
            var response = _handler.Handle(request);
            response.Items.Count().ShouldBe(5);
        }

        [Fact]
        public void Test_QueryContacts_Search_Id()
        {
            var request = new QueryContacts { Search = "1" };
            var response = _handler.Handle(request);
            response.Items.Single().Id.ShouldBe(1);
        }

        [Fact]
        public void Test_QueryContacts_Search_FirstName()
        {
            var request = new QueryContacts { Search = "D" };
            var response = _handler.Handle(request);
            response.Items.Single().Id.ShouldBe(2);
        }

        [Fact]
        public void Test_QueryContacts_Search_LastName()
        {
            var request = new QueryContacts { Search = "H" };
            var response = _handler.Handle(request);
            response.Items.Single().Id.ShouldBe(3);
        }

        [Fact]
        public void Test_QueryContacts_Search_Email()
        {
            var request = new QueryContacts { Search = "L" };
            var response = _handler.Handle(request);
            response.Items.Single().Id.ShouldBe(4);
        }

        [Fact]
        public void Test_QueryContacts_Search_Phone()
        {
            var request = new QueryContacts { Search = "0" };
            var response = _handler.Handle(request);
            response.Items.Single().Id.ShouldBe(5);
        }

        [Fact]
        public void Test_QueryContacts_Paging()
        {
            var request = new QueryContacts { PageNumber = 3, PageSize = 1 };
            var response = _handler.Handle(request);
            response.Items.Single().Id.ShouldBe(3);
        }
    };
}
