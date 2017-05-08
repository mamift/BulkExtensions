using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    /// <summary>
    /// 
    /// </summary>
    internal class BulkUpdate : IBulkOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <param name="options"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        int IBulkOperation.CommitTransaction<TEntity>(IDbContextWrapper context, IEnumerable<TEntity> collection, Options options)
        {
            var tmpTableName = context.EntityMapping.RandomTableName();
            var entityList = collection.ToList();
            var affectedRows = 0;
            if (!entityList.Any())
            {
                return affectedRows;
            }

            try
            {
                //Create temporary table.
                var command = context.EntityMapping.CreateTempTable(tmpTableName, OperationType.Update);
                context.ExecuteSqlCommand(command);

                //Bulk inset data to temporary temporary table.
                context.BulkInsertToTable(entityList, tmpTableName, OperationType.Update);

                //Copy data from temporary table to destination table.
                command = $"MERGE INTO {context.EntityMapping.FullTableName} WITH (HOLDLOCK) AS Target USING {tmpTableName} AS Source " +
                          $"{context.EntityMapping.PrimaryKeysComparator()} WHEN MATCHED THEN UPDATE {context.EntityMapping.BuildUpdateSet()}; " +
                          SqlHelper.GetDropTableCommand(tmpTableName);

                affectedRows = context.ExecuteSqlCommand(command);

                //Commit if internal transaction exists.
                //context.UpdateEntityState(entityList);
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
