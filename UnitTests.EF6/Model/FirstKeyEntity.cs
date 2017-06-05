using System.ComponentModel.DataAnnotations;

namespace UnitTests.EF6.Model
{
    public class FirstKeyEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}