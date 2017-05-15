using System;

namespace EntityFramework.BulkExtensions
{
    [Flags]
    internal enum UpdateOptions
    {
        Default = 1,
        KeepForeinKeys = 4
    }
}