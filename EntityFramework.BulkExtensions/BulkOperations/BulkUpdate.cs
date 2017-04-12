using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using EntityFramework.BulkExtensions.Extensions;
using EntityFramework.BulkExtensions.Helpers;
using EntityFramework.BulkExtensions.Mapping;
using EntityFramework.BulkExtensions.Operations;

namespace EntityFramework.BulkExtensions.BulkOperations
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
        /// <param name="identity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        int IBulkOperation.CommitTransaction<TEntity>(DbContext context, IEnumerable<TEntity> collection, Identity identity)
        {
            var mapping = context.Mapping<TEntity>(OperationType.Update);
            var tmpTableName = mapping.RandomTableName();
            var entityList = collection.ToList();
            var database = context.Database;
            var affectedRows = 0;
            if (!entityList.Any())
            {
                return affectedRows;
            }

            //Creates inner transaction for the scope of the operation if the context doens't have one.
            var transaction = context.InternalTransaction();
            try
            {
                //Cconvert entity collection into a DataTable
                var dataTable = entityList.ToDataTable(mapping);
                //Create temporary table.
                var command = mapping.CreateTempTable(tmpTableName);
                database.ExecuteSqlCommand(command);

                //Bulk inset data to temporary temporary table.
                database.BulkInsertToTable(dataTable, tmpTableName, SqlBulkCopyOptions.Default);

                //Copy data from temporary table to destination table.
                command = $"MERGE INTO {mapping.FullTableName} WITH (HOLDLOCK) AS Target USING {tmpTableName} AS Source " +
                          $"{mapping.PrimaryKeysComparator()} WHEN MATCHED THEN UPDATE {mapping.BuildUpdateSet()}; " +
                          SqlHelper.GetDropTableCommand(tmpTableName);

                affectedRows = database.ExecuteSqlCommand(command);

                //Commit if internal transaction exists.
                transaction?.Commit();
                //context.UpdateEntityState(entityList);
                return affectedRows;
            }
            catch (Exception)
            {
                //Rollback if internal transaction exists.
                transaction?.Rollback();
                throw;
            }
        }
    }
}
