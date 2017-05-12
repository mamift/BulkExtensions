using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
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
                if (options.HasFlag(BulkOptions.OutputIdentity))
                {
                    var tmpTableName = context.EntityMapping.RandomTableName();
                    //Create temporary table.
                    context.ExecuteSqlCommand(context.EntityMapping.CreateTempTable(tmpTableName, Operation.Insert, options));

                    //Bulk inset data to temporary temporary table.
                    context.BulkInsertToTable(entityList, tmpTableName, Operation.Insert, options);

                    var tmpOutputTableName = context.EntityMapping.RandomTableName();
                    //Copy data from temporary table to destination table with ID output to another temporary table.
                    var commandText = context.EntityMapping.GetInsertIntoStagingTableCmd(tmpOutputTableName,
                        tmpTableName, context.EntityMapping.Pks.First().ColumnName, Operation.Insert, options);
                    context.ExecuteSqlCommand(commandText);

                    //Load generated IDs from temporary output table into the entities.
                    context.LoadFromTmpOutputTable(tmpOutputTableName, context.EntityMapping.Pks.First(), entityList, Operation.Insert);
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