using System.Data.Entity.Migrations;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// AutomaticMigrations allows EntityFramework to keep our database schema
    ///   in sync with our code models automatically. It will create a local database
    ///   called CommanderDemo.Domain.ContactDb automatically since we aren't providing
    ///   a connectionstring to ContactDb.
    /// </summary>
    internal class MigrationsConfig : DbMigrationsConfiguration<ContactDb>
    {
        public MigrationsConfig()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ContactDb db)
        {
            AddUsers(db);
            base.Seed(db);
        }

        private static void AddUsers(ContactDb db)
        {
            //Let's add a default user to the database so we can log in
            db.Users.AddOrUpdate(x => x.Username,
                new User
                {
                    Username = "Admin",
                    Password = "password",
                    IsActive = true,
                }
            );
        }
    };
}
