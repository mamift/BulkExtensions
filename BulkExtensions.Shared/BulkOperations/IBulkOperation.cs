using System.Collections.Generic;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Flags;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    internal interface IBulkOperation
    {
        int CommitTransaction<TEntity>(IDbContextWrapper context, IEnumerable<TEntity> collection, BulkOptions options = BulkOptions.Default) where TEntity : class;
    }
}