using System.ComponentModel.DataAnnotations;

namespace UnitTests.EFCore.Model
{
    public class FirstKeyEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}