using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Helpers;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    /// <summary>
    /// 
    /// </summary>
    internal class BulkDelete : IBulkOperation
    {
        ///  <summary>
        ///  </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <param name="options"></param>
        /// <typeparam name="TEntity"></typeparam>
        ///  <returns></returns>
        int IBulkOperation.CommitTransaction<TEntity>(IDbContextWrapper context, IEnumerable<TEntity> collection, BulkOptions options)
        {
            var tmpTableName = context.EntityMapping.RandomTableName();
            var entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                //Create temporary table with only the primary keys.
                context.ExecuteSqlCommand(context.EntityMapping.CreateTempTable(tmpTableName, Operation.Delete, options));

                //Bulk inset data to temporary table.
                context.BulkInsertToTable(entityList, tmpTableName, Operation.Delete, options);

                //Merge delete items from the target table that matches ids from the temporary table.
                var affectedRows = context.ExecuteSqlCommand(context.BuildMergeCommand(tmpTableName, Operation.Delete));

                //Commit if internal transaction exists.
                context.Commit();
                return affectedRows;
            }
            catch (Exception)
            {
                //Rollback if internal transaction exists.
                context.Rollback();
                throw;
            }
        }
    }
}