using System;

namespace EntityFramework.BulkExtensions
{
    [Flags]
    public enum UpdateOptions
    {
        Default = 1,
        OutputComputed = 4
    }
}