using System.ComponentModel.DataAnnotations;

namespace UnitTests.EF6.Model
{
    public class ConcurrencyTokenEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}