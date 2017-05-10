using System.Collections.Generic;
using BulkExtensions.Options;
using EntityFramework.BulkExtensions.Commons.Context;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    internal interface IBulkOperation
    {
        int CommitTransaction<TEntity>(IDbContextWrapper context, IEnumerable<TEntity> collection, Options options = Options.Default) where TEntity : class;
    }
}