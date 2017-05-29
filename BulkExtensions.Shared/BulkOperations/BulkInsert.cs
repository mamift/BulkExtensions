using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Helpers;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    internal class BulkInsert : IBulkOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        int IBulkOperation.CommitTransaction<TEntity>(IDbContextWrapper context, IEnumerable<TEntity> collection,
            BulkOptions options)
        {
            var entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                //Return generated IDs for bulk inserted elements.
                if (context.EntityMapping.WillOutputGeneratedValues(options))
                {
                    var generatedColumns = context.EntityMapping.GetPropertiesByOptions(options).ToList();

                    //Create temporary table.
                    var tmpTableName = context.EntityMapping.RandomTableName();
                    context.ExecuteSqlCommand(context.EntityMapping.BuildStagingTableCommand(tmpTableName, Operation.Insert, options));

                    //Bulk inset data to temporary temporary table.
                    context.BulkInsertToTable(entityList, tmpTableName, Operation.Insert, options);

                    //Create output table
                    var outputTableName = context.EntityMapping.RandomTableName();
                    context.ExecuteSqlCommand(SqlHelper.BuildOutputTableCommand(outputTableName, context.EntityMapping, generatedColumns));

                    //Copy data from temporary table to destination table with ID output to another temporary table.
                    var mergeCommand = context.BuildMergeCommand(tmpTableName, Operation.Insert);
                    mergeCommand += SqlHelper.BuildOutputValues(outputTableName, generatedColumns);
                    mergeCommand += SqlHelper.GetDropTableCommand(tmpTableName);
                    context.ExecuteSqlCommand(mergeCommand);

                    //Load generated values from temporary output table into the entities.
                    context.LoadFromOutputTable(outputTableName, generatedColumns, entityList);
                }
                else
                {
                    //Bulk inset data to temporary destination table.
                    context.BulkInsertToTable(entityList, context.EntityMapping.FullTableName, Operation.Insert, options);
                }

                //Commit if internal transaction exists.
                context.Commit();
                return entityList.Count;
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