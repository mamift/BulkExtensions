using System.ComponentModel.DataAnnotations;

namespace UnitTests.EF6.Model
{
    public class Client : Person
    {
        [MaxLength(20)]
        public string Category { get; set; }
    }
}