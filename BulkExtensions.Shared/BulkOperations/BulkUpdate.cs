using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Helpers;

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
                //Create temporary table.
                context.ExecuteSqlCommand(context.EntityMapping.CreateTempTable(tmpTableName, Operation.Update, options));                

                //Bulk inset data to temporary temporary table.
                context.BulkInsertToTable(entityList, tmpTableName, Operation.Update, options);

                //Copy data from temporary table to destination table.
                var command = context.BuildMergeCommand(tmpTableName, Operation.Update);
                command += $";{SqlHelper.GetDropTableCommand(tmpTableName)}";
                var affectedRows = context.ExecuteSqlCommand(command);

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
