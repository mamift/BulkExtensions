
using System.ComponentModel.DataAnnotations;

namespace UnitTests.EF6.Model
{
    public class IdentityOnlyAutoEntity
    {
        [Key]
        public int Id { get; set; }
    }

    public class IdentityOnlyEntity
    {
        [Key]
        public string Id { get; set; }
    }
}