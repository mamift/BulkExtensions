using System.Collections.Generic;
using System.Data.SqlClient;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Helpers;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class BulkInsertExtension
    {
        internal static void BulkInsertToTable<TEntity>(this IDbContextWrapper context, IEnumerable<TEntity> entities,
            string tableName, OperationType operationType, BulkOptions options) where TEntity : class
        {
            var properties = context.EntityMapping.Properties
                .FilterPropertiesByOperation(operationType);
            var dataReader = entities.ToDataReader(context.EntityMapping, operationType, options);

            using (var bulkcopy = new SqlBulkCopy((SqlConnection) context.Connection,
                SqlBulkCopyOptions.Default | SqlBulkCopyOptions.KeepIdentity,
                (SqlTransaction) context.Transaction))
            {
                foreach (var column in properties)
                {
                    bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                bulkcopy.DestinationTableName = tableName;
                bulkcopy.BulkCopyTimeout = context.Connection.ConnectionTimeout;
                bulkcopy.WriteToServer(dataReader);
            }
        }
    }
}