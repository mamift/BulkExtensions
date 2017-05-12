using System;

namespace EntityFrameworkCore.BulkExtensions
{
    [Flags]
    public enum InsertOptions
    {
        Default = 1,
        OutputIdentity = 2
    }
}