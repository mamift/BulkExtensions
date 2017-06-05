using System;
using System.ComponentModel.DataAnnotations;

namespace UnitTests.NetStandard.Model
{
    public class SimpleModel
    {
        [Key]
        public int Id { get; set; }

        public string StringProperty { get; set; }
        public DateTime DateTime { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public SimpleEnum Type { get; set; }
    }

    public enum SimpleEnum
    {
        First,
        Second,
        Third
    }
}