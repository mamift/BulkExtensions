using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using EntityFramework.BulkExtensions.Extensions;
using EntityFramework.BulkExtensions.Helpers;
using EntityFramework.BulkExtensions.Metadata;
using EntityFramework.BulkExtensions.Operations;

namespace EntityFramework.BulkExtensions.BulkOperations
{
    internal class BulkInsert : IBulkOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        int IBulkOperation.CommitTransaction<TEntity>(DbContext context, IEnumerable<TEntity> collection, Identity identity)
        {
            var metadata = context.Metadata<TEntity>(OperationType.Insert);
            var tmpTableName = metadata.RandomTableName();
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
                var dataTable = entityList.ToDataTable(metadata);

                //Return generated IDs for bulk inserted elements.
                if (identity == Identity.Output)
                {
                    //Create temporary table.
                    var command = metadata.CreateTempTable(tmpTableName);
                    database.ExecuteSqlCommand(command);

                    //Bulk inset data to temporary temporary table.
                    database.BulkInsertToTable(dataTable, tmpTableName, SqlBulkCopyOptions.Default);

                    var tmpOutputTableName = metadata.RandomTableName();
                    //Copy data from temporary table to destination table with ID output to another temporary table.
                    var commandText = metadata.GetInsertIntoStagingTableCmd(tmpOutputTableName, tmpTableName, metadata.Pks.First().ColumnName);
                    database.ExecuteSqlCommand(commandText);

                    //Load generated IDs from temporary output table into the entities.
                    database.LoadFromTmpOutputTable(tmpOutputTableName, metadata.Pks.First().ColumnName, entityList);
                    //context.UpdateEntityState(entityList);
                }
                else
                {
                    //Bulk inset data to temporary destination table.
                    database.BulkInsertToTable(dataTable, metadata.FullTableName, SqlBulkCopyOptions.Default);
                }

                affectedRows = dataTable.Rows.Count;

                //Commit if internal transaction exists.
                transaction?.Commit();
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