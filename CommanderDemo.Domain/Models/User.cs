using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// EntityFramework code first POCO model.
    /// The ContactDb.Users collection tells EntityFramework which
    /// DbContext schema this model belongs to.
    /// </summary>
    [Table("User")]
    internal class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Username { get; set; }

        [StringLength(100)]
        public string Password { get; set; }

        public bool IsActive { get; set; }
    };
}
