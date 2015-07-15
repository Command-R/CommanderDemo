using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommanderDemo.Domain
{
    /// <summary>
    /// EntityFramework code first POCO model.
    /// The ContactDb.Contacts collection tells EntityFramework which
    /// DbContext schema this model belongs to.
    /// </summary>
    [Table("Contact")]
    internal class Contact
    {
        [Key]
        public int Id { get; set; }

        [StringLength(25)]
        public string FirstName { get; set; }

        [StringLength(25)]
        public string LastName { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(25)]
        public string PhoneNumber { get; set; }
    };
}
