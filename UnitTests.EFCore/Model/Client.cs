using System.ComponentModel.DataAnnotations;

namespace UnitTests.NetStandard.Model
{
    public class Client : Person
    {
        [MaxLength(20)]
        public string Category { get; set; }
    }
}