using System.Data.Entity;
using System.Diagnostics;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// We keep our models and database as internal because *everything* is only ever
    /// exposed via the command view models. We're also going to tell EntityFramework to
    /// use our MigrationsConfig which configures AutomaticMigrations to keep our db schema
    /// in sync with our model changes automatically on app startup.
    /// </summary>
    internal class ContactDb : DbContext
    {
        public ContactDb()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ContactDb, MigrationsConfig>());
            Database.Log = sql => Debug.WriteLine(sql);
        }

        public virtual IDbSet<Contact> Contacts { get; set; }
        public virtual IDbSet<User> Users { get; set; }
    };
}
