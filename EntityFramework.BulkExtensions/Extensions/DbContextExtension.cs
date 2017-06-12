using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Mapping;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class DbContextExtension
    {
        internal static DbContextWrapper GetContextWrapper<TEntity>(this DbContext context) where TEntity : class
        {
            var database = context.Database;
            return new DbContextWrapper(database.Connection, database.CurrentTransaction?.UnderlyingTransaction,
                context.Mapping<TEntity>(), context.Database.CommandTimeout);
        }

        internal static DbContextWrapper GetContextWrapper(this DbContext context, Type type)
        {
            var database = context.Database;
            return new DbContextWrapper(database.Connection, database.CurrentTransaction?.UnderlyingTransaction,
                context.Mapping<object>(type), context.Database.CommandTimeout);
        }

        internal static IEnumerable<IGrouping<Type, DbEntityEntry>> GetEntriesByState(this DbContext context,
            EntityState state)
        {
            return context.ChangeTracker
                .Entries()
                .Where(entry => state.HasFlag(entry.State))
                .GroupBy(entry => entry.Entity.GetType())
                .ToList();
        }
    }
}