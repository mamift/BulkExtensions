using System.ComponentModel.DataAnnotations;

namespace EntityFramework.BulkExtensions.UnitTests.Model
{
    public class Client : Person
    {
        [MaxLength(20)]
        public string Category { get; set; }
    }
}