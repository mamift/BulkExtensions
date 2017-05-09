using System.Data.Entity;
using EntityFramework.BulkExtensions.Commons.Mapping;
using EntityFramework.BulkExtensions.Mapping;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class DbContextExtension
    {
        internal static DbContextWrapper GetContextWrapper<TEntity>(this DbContext context) where TEntity : class
        {
            var database = context.Database;            
            return new DbContextWrapper(database.Connection, database.CurrentTransaction?.UnderlyingTransaction,
                context.Mapping<TEntity>());
        }
    }
}