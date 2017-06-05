using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTests.NetStandard.Model
{
    [Table("CustomSchemaEntity", Schema = "cse")]
    public class CustomSchemaEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}