using System;
using System.Data.Entity;
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
    }
}