using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;

namespace EntityFramework.BulkExtensions.Extensions
{
    public static class ObjectContextExtension
    {
        public static IExtendedDataRecord UsableValues(this ObjectStateEntry entry)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Modified:
                    return entry.CurrentValues;
                case EntityState.Deleted:
                    return (IExtendedDataRecord)entry.OriginalValues;
                default:
                    throw new InvalidOperationException("This entity state should not exist.");
            }
        }
    }
}