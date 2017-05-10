using System;

namespace EntityFramework.BulkExtensions
{
    [Flags]
    public enum Options
    {
        Default = 1,
        OutputIdentity = 2
    }
}