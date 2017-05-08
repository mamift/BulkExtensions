using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class BulkInsertExtension
    {
        internal static void BulkInsertToTable<TEntity>(this IDbContextWrapper context, IEnumerable<TEntity> entities,
            string tableName, OperationType operationType) where TEntity : class
        {
            var dataReader = entities.ToDataReader(context.EntityMapping, operationType);
            using (var bulkcopy = new SqlBulkCopy((SqlConnection) context.Connection, SqlBulkCopyOptions.Default,
                (SqlTransaction) context.Transaction))
            {
                foreach (var column in context.EntityMapping.Properties.FilterProperties(operationType))
                {
                    bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                bulkcopy.DestinationTableName = tableName;
                bulkcopy.BulkCopyTimeout = context.Connection.ConnectionTimeout;
                bulkcopy.WriteToServer(dataReader);
            }
        }

        internal static void BulkInsertToTable2<TEntity>(this IDbContextWrapper context, IEnumerable<TEntity> entities,
            string tableName, OperationType operationType) where TEntity : class
        {

        }

        private static string BuildInsertIntoSet(IEnumerable<IPropertyMapping> columns, string tableName)
        {
            var command = new StringBuilder();
            var insertColumns = new List<string>();

            command.Append("INSERT INTO ");
            command.Append(tableName);
            command.Append(" (");

            foreach (var column in columns)
                if (!column.IsPk)
                    insertColumns.Add($"[{column}]");

            command.Append(string.Join(", ", insertColumns));
            command.Append(") ");
            command.Append("Values");
            command.Append(" (");
            foreach (var column in columns)
                if (!column.IsPk)
                    insertColumns.Add($"[{column}]");

            return command.ToString();
        }
    }
}