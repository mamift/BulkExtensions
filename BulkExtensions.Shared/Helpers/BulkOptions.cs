using System;

namespace EntityFramework.BulkExtensions.Commons.Helpers
{
    [Flags]
    public enum BulkOptions
    {
        Default = 1,
        OutputIdentity = 2,
        KeepForeingKeys = 4
    }
}