using System.ComponentModel.DataAnnotations;

namespace UnitTests.EFCore.Model
{
    public class Client : Person
    {
        [MaxLength(20)]
        public string Category { get; set; }
    }
}