using System.ComponentModel.DataAnnotations;

namespace BulkExtensions.Tests.Model
{
    public class Client : Person
    {
        [MaxLength(20)]
        public string Category { get; set; }
    }
}