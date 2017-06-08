using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Helpers;

namespace BulkExtensions.Shared.BulkOperations
{
    internal static class BulkOperation
    {
        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <param name="operation"></param>
        /// <param name="options"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        internal static int CommitTransaction<TEntity>(this IDbContextWrapper context, IEnumerable<TEntity> collection, Operation operation,
            BulkOptions options = BulkOptions.Default) where TEntity : class
        {
            var stagingTableName = context.EntityMapping.RandomTableName();
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
                    : null;
                var generatedColumns = willOutputGeneratedValues
                    ? context.EntityMapping.GetPropertiesByOptions(options).ToList()
                    : null;

                //Create temporary table.
                context.ExecuteSqlCommand(context.EntityMapping
                    .BuildStagingTableCommand(stagingTableName, operation, options));

                //Bulk inset data to temporary staging table.
                context.BulkInsertToTable(entityList, stagingTableName, operation, options);

                if (willOutputGeneratedValues)
                {
                    context.ExecuteSqlCommand(SqlHelper.BuildOutputTableCommand(outputTableName,
                        context.EntityMapping, generatedColumns));
                }

                //Copy data from temporary table to destination table.
                var mergeCommand = context.BuildMergeCommand(stagingTableName, operation);
                if (willOutputGeneratedValues)
                {
                    mergeCommand += SqlHelper.BuildMergeOutputSet(outputTableName, generatedColumns);
                }
                mergeCommand += SqlHelper.GetDropTableCommand(stagingTableName);
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
