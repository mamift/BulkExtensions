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
    /// </summary>
    internal class BulkInsertOrUpdate : IBulkOperation
    {
        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <param name="options"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        int IBulkOperation.CommitTransaction<TEntity>(IDbContextWrapper context, IEnumerable<TEntity> collection,
            BulkOptions options)
        {
            var tmpTableName = context.EntityMapping.RandomTableName();
            var willOutputGeneratedValues = context.EntityMapping.WillOutputGeneratedValues(options);
            var entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                var outputTableName = willOutputGeneratedValues
                    ? context.EntityMapping.RandomTableName()
                    : string.Empty;
                var generatedColumns = willOutputGeneratedValues
                    ? context.EntityMapping.GetPropertiesByOptions(options).ToList()
                    : null;
                //Create temporary table.
                context.ExecuteSqlCommand(context.EntityMapping.BuildStagingTableCommand(tmpTableName, Operation.InsertOrUpdate, options));

                //Bulk inset data to temporary temporary table.
                context.BulkInsertToTable(entityList, tmpTableName, Operation.InsertOrUpdate, options);
                if (willOutputGeneratedValues)
                {
                    context.ExecuteSqlCommand(SqlHelper.BuildOutputTableCommand(outputTableName, context.EntityMapping, generatedColumns));
                }

                //Copy data from temporary table to destination table.
                var mergeCommand = context.BuildMergeCommand(tmpTableName, Operation.InsertOrUpdate);
                mergeCommand += willOutputGeneratedValues ? SqlHelper.BuildOutputValues(outputTableName, generatedColumns) : string.Empty;
                mergeCommand += SqlHelper.GetDropTableCommand(tmpTableName);
                var affectedRows = context.ExecuteSqlCommand(mergeCommand);

                if (willOutputGeneratedValues)
                {
                    //Load generated values from temporary output table into the entities.
                    context.LoadFromOutputTable(outputTableName, generatedColumns, entityList);
                }

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