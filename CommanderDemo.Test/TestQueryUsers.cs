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
    public class TestQueryUsers
    {
        private readonly QueryUsers.Handler _handler;

        public TestQueryUsers()
        {
            var users = new[]
            {
                new User {Id = 1, IsActive = true},
                new User {Id = 2, IsActive = false},
                new User {Id = 3, IsActive = true},
                new User {Id = 4, IsActive = false},
            }.ToDbSet();

            var db = A.Fake<ContactDb>();
            A.CallTo(() => db.Users).Returns(users);

            _handler = new QueryUsers.Handler(db);
        }

        [Fact]
        public void Test_QueryUsers_Excludes_Inactive_By_Default()
        {
            var queryUsers = new QueryUsers();
            var response = _handler.Handle(queryUsers);
            response.Result.Items.Count().ShouldBe(2);
        }

        [Fact]
        public void Test_QueryUsers_Includes_Inactive_If_True()
        {
            var queryUsers = new QueryUsers { Inactive = true };
            var response = _handler.Handle(queryUsers);
            response.Result.Items.Count().ShouldBe(4);
        }

        [Fact]
        public void Test_QueryUsers_Paging()
        {
            var queryUsers = new QueryUsers { Inactive = true, PageNumber = 3, PageSize = 1 };
            var response = _handler.Handle(queryUsers);
            response.Result.Items.Single().Id.ShouldBe(3);
        }
    };
}
