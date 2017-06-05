using System;
using System.Linq;
using UnitTests.NetStandard.Model;

namespace UnitTests.NetStandard.Helpers
{
    public static class Helper
    {
        private static readonly Random Random = new Random();
        
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*-_+=";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)])
                .ToArray());
        }

        public static int RandomInt()
        {
            return Random.Next(int.MaxValue);
        }

        public static SimpleEnum RandomEnum()
        {
            return (SimpleEnum) Random.Next(3);
        }
        
        public static double RandomDouble(double minimum, double maximum)
        { 
            return Random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}