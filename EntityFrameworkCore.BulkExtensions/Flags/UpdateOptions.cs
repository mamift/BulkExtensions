using System;

namespace EntityFrameworkCore.BulkExtensions
{
    [Flags]
    internal enum UpdateOptions
    {
        Default = 1,
        KeepForeinKeys = 4
    }
}