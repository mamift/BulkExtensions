
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTests.NetStandard.Model
{
    public class IdentityOnlyAutoEntity
    {
        [Key]
        public int Id { get; set; }
    }

    public class IdentityOnlyEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
    }
}