using System;

namespace UnitTests.NetStandard.Model
{
    public class SimpleModel
    {
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