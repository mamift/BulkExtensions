using System;

namespace EntityFramework.BulkExtensions
{
    [Flags]
    public enum InsertOptions
    {
        Default = 1,
        OutputIdentity = 2,
        OutputComputed = 4
    }
}