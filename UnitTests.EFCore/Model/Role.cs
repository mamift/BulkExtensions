using System.ComponentModel.DataAnnotations;

namespace UnitTests.NetStandard.Model
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}