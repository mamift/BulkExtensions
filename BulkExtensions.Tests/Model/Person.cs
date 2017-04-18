using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkExtensions.Tests.Model
{
    public class Person
    {
        [Key, Column("PersonId")]
        public int Id { get; set; }
        [Required, StringLength(25)]
        public string Name { get; set; }
        public DateTime Birthday { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}