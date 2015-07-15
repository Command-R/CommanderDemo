using System;
using CommanderDemo.Domain;
using CommandR;

//Project Reference: CommanderDemo.Domain
//Solution Nuget: Command-R
namespace CommanderDemo.Console
{
    internal class Program
    {
        private static void Main()
        {
            var client = new JsonRpcClient("http://localhost:64862/jsonrpc");
            client.Authorization = client.Send(new LoginUser
            {
                Username = "Admin",
                Password = "password",
            });

            var id = client.Send(new SaveContact
            {
                FirstName = "Test",
                LastName = Guid.NewGuid().ToString("N").Substring(10),
                Email = "test@example.com",
                PhoneNumber = "555-1212",
            });

            System.Console.WriteLine(id);
            System.Console.ReadKey();
        }
    };
}
