using System;

namespace EntityFramework.BulkExtensions.Commons.Helpers
{
    [Flags]
    public enum Options
    {
        Default = 1,
        OutputIdentity = 2
    }

    internal enum OperationType
    {
        Insert,
        Update,
        Delete
    }
}