using System;
using System.ComponentModel.DataAnnotations;

namespace BulkExtensions.Tests.Model
{
    public class Person
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
    }
}