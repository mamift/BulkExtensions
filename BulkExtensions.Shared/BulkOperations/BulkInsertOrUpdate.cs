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
    internal class BulkInsertOrUpdate : IBulkOperation
    {
        /// <summary>
        /// 
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
            var outputTableName = options.HasFlag(BulkOptions.OutputIdentity)
                ? context.EntityMapping.RandomTableName()
                : string.Empty;
            var entityList = collection.ToList();
            if (!entityList.Any())
            {
                return entityList.Count;
            }

            try
            {
                var pk = context.EntityMapping.Pks.FirstOrDefault(pkey => pkey.IsStoreGenerated);
                //Create temporary table.
                context.ExecuteSqlCommand(context.EntityMapping.CreateTempTable(tmpTableName, Operation.InsertOrUpdate, options));

                //Bulk inset data to temporary temporary table.
                context.BulkInsertToTable(entityList, tmpTableName, Operation.InsertOrUpdate, options);

                if (options.HasFlag(BulkOptions.OutputIdentity) && pk != null)
                {
                    context.ExecuteSqlCommand(SqlHelper.CreateOutputTableCmd(outputTableName, pk.ColumnName, Operation.InsertOrUpdate));
                }

                //Copy data from temporary table to destination table.
                var mergeCommand = context.BuildMergeCommand(tmpTableName, Operation.InsertOrUpdate);
                if (options.HasFlag(BulkOptions.OutputIdentity) && pk != null)
                {
                    mergeCommand += SqlHelper.BuildOutputId(outputTableName, pk.ColumnName);
                }
                mergeCommand += SqlHelper.GetDropTableCommand(tmpTableName);
                var affectedRows = context.ExecuteSqlCommand(mergeCommand);

                if (options.HasFlag(BulkOptions.OutputIdentity) && pk != null)
                {
                    context.LoadFromTmpOutputTable(outputTableName, pk, entityList);
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