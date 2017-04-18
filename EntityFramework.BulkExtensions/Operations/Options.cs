using System;

namespace EntityFramework.BulkExtensions.Operations
{
    [Flags]
    public enum Options
    {
        Default = 1,
        OutputIdentity = 2
    }
}