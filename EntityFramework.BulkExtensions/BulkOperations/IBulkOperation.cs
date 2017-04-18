using System.Collections.Generic;
using System.Data.Entity;
using EntityFramework.BulkExtensions.Operations;

// ReSharper disable once CheckNamespace

namespace EntityFramework.BulkExtensions.BulkOperations
{
    internal interface IBulkOperation
    {
        int CommitTransaction<TEntity>(DbContext context, IEnumerable<TEntity> collection, Options options = Options.Default) where TEntity : class;
    }
}