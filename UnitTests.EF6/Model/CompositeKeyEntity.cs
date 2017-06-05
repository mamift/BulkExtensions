using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTests.EF6.Model
{
    public class CompositeKeyEntity
    {
        [Column(Order = 0), Key]
        public int FirstKeyEntityId { get; set; }
        [Column(Order = 1), Key]
        public int SecondKeyEntityId { get; set; }

        public virtual FirstKeyEntity FirstKeyEntity { get; set; }
        public virtual SecondKeyEntity SecondKeyEntity { get; set; }

        public string Name { get; set; }
    }
}