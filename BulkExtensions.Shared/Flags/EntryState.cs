using System;
namespace EntityFramework.BulkExtensions.Commons.Flags
{
    [Flags]
    public enum EntryState
    {
        Added = 1,
        Modified = 2,
        Deleted = 4,
        Unchanged = 8
    }
}
