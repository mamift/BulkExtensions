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
            var mapping = context.Mapping<TEntity>(OperationType.Insert);
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

                //Return generated IDs for bulk inserted elements.
                if (identity == Identity.Output)
                {
                    var tmpTableName = mapping.RandomTableName();
                    //Create temporary table.
                    var command = mapping.CreateTempTable(tmpTableName);
                    database.ExecuteSqlCommand(command);

                    //Bulk inset data to temporary temporary table.
                    database.BulkInsertToTable(dataTable, tmpTableName, SqlBulkCopyOptions.Default);

                    var tmpOutputTableName = mapping.RandomTableName();
                    //Copy data from temporary table to destination table with ID output to another temporary table.
                    var commandText = mapping.GetInsertIntoStagingTableCmd(tmpOutputTableName, tmpTableName, mapping.Pks.First().ColumnName);
                    database.ExecuteSqlCommand(commandText);

                    //Load generated IDs from temporary output table into the entities.
                    database.LoadFromTmpOutputTable(tmpOutputTableName, mapping.Pks.First(), entityList);
                    //context.UpdateEntityState(entityList);
                }
                else
                {
                    //Bulk inset data to temporary destination table.
                    database.BulkInsertToTable(dataTable, mapping.FullTableName, SqlBulkCopyOptions.Default);
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