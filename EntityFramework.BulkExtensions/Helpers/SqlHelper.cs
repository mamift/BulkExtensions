using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using EntityFramework.BulkExtensions.BulkOperations;
using EntityFramework.BulkExtensions.Extensions;
using EntityFramework.BulkExtensions.Mapping;

namespace EntityFramework.BulkExtensions.Helpers
{
    /// <summary>
    /// </summary>
    internal static class SqlHelper
    {
        private const string Source = "Source";
        private const string Target = "Target";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        internal static string RandomTableName(this EntityMapping mapping)
        {
            return $"[{mapping.Schema}].[_{mapping.TableName}_{GuidHelper.GetRandomTableGuid()}]";
        }

        /// <summary>
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="tableName"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        internal static string CreateTempTable(this EntityMapping mapping, string tableName, OperationType operationType)
        {
            var columns = mapping.Properties.FilterProperties(operationType).ToList();
            var command = new StringBuilder();

            command.Append($"CREATE TABLE {tableName}(");

            var paramList = columns
                .Select(column => $"[{column.ColumnName}] {column.GetSchemaType(column.DbType)}")
                .ToList();
            var paramListConcatenated = string.Join(", ", paramList);

            command.Append(paramListConcatenated);
            command.Append(");");

            return command.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="database"></param>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        /// <param name="sqlBulkCopyOptions"></param>
        internal static void BulkInsertToTable(this Database database, DataTable dataTable, string tableName,
            SqlBulkCopyOptions sqlBulkCopyOptions)
        {
            using (
                var bulkcopy = new SqlBulkCopy((SqlConnection)database.Connection, sqlBulkCopyOptions,
                    (SqlTransaction)database.CurrentTransaction.UnderlyingTransaction))
            {
                foreach (var dataTableColumn in dataTable.Columns)
                {
                    bulkcopy.ColumnMappings.Add(dataTableColumn.ToString(), dataTableColumn.ToString());
                }

                bulkcopy.DestinationTableName = tableName;
                bulkcopy.BulkCopyTimeout = database.Connection.ConnectionTimeout;
                bulkcopy.WriteToServer(dataTable);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static string GetDropTableCommand(string tableName)
        {
            return $"DROP TABLE {tableName};";
        }

        /// <summary>
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        internal static string BuildUpdateSet(this EntityMapping mapping)
        {
            var command = new StringBuilder();
            var parameters = new List<string>();

            command.Append("SET ");

            foreach (var column in mapping.Properties.Where(propertyMapping => !propertyMapping.IsHierarchyMapping))
            {
                if (column.IsPk) continue;

                parameters.Add($"[{Target}].[{column.ColumnName}] = [{Source}].[{column.ColumnName}]");
            }

            command.Append(string.Join(", ", parameters) + " ");

            return command.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        internal static string PrimaryKeysComparator(this EntityMapping mapping)
        {
            var keys = mapping.Pks.ToList();
            var command = new StringBuilder();
            var firstKey = keys.First();

            command.Append($"ON [{Target}].[{firstKey.ColumnName}] = [{Source}].[{firstKey.ColumnName}] ");
            keys.Remove(firstKey);

            if (keys.Any())
                foreach (var key in keys)
                    command.Append($"AND [{Target}].[{key.ColumnName}] = [{Source}].[{key.ColumnName}]");

            return command.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="tmpOutputTableName"></param>
        /// <param name="tmpTableName"></param>
        /// <param name="identityColumn"></param>
        /// <returns></returns>
        internal static string GetInsertIntoStagingTableCmd(this EntityMapping mapping, string tmpOutputTableName,
            string tmpTableName, string identityColumn)
        {
            var columns = mapping.Properties.Select(propertyMapping => propertyMapping.ColumnName).ToList();

            var comm = GetOutputCreateTableCmd(tmpOutputTableName, identityColumn)
                       + BuildInsertIntoSet(columns, identityColumn, mapping.FullTableName)
                       + $"OUTPUT INSERTED.{identityColumn} INTO "
                       + tmpOutputTableName + $"([{identityColumn}]) "
                       + BuildSelectSet(columns, identityColumn)
                       + $" FROM {tmpTableName} AS Source; "
                       + GetDropTableCommand(tmpTableName);

            return comm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="tmpOutputTableName"></param>
        /// <param name="propertyMapping"></param>
        /// <param name="items"></param>
        internal static void LoadFromTmpOutputTable<TEntity>(this Database context, string tmpOutputTableName,
            PropertyMapping propertyMapping, IList<TEntity> items)
        {
            var command = $"SELECT {propertyMapping.ColumnName} FROM {tmpOutputTableName} ORDER BY {propertyMapping.ColumnName};";
            var identities = context.SqlQuery<int>(command).ToList();

            foreach (var result in identities)
            {
                var index = identities.IndexOf(result);
                var property = items[index].GetType().GetProperty(propertyMapping.PropertyName);

                if (property != null && property.CanWrite)
                    property.SetValue(items[index], result, null);

                else
                    throw new Exception();
            }

            command = GetDropTableCommand(tmpOutputTableName);
            context.ExecuteSqlCommand(command);
        }

        private static string BuildSelectSet(IEnumerable<string> columns, string identityColumn)
        {
            var command = new StringBuilder();
            var selectColumns = new List<string>();

            command.Append("SELECT ");

            foreach (var column in columns.ToList())
            {
                if (((identityColumn == null) || (column == identityColumn)) && (identityColumn != null)) continue;
                selectColumns.Add($"[{Source}].[{column}]");
            }

            command.Append(string.Join(", ", selectColumns));

            return command.ToString();
        }

        private static string BuildInsertIntoSet(IEnumerable<string> columns, string identityColumn, string tableName)
        {
            var command = new StringBuilder();
            var insertColumns = new List<string>();

            command.Append("INSERT INTO ");
            command.Append(tableName);
            command.Append(" (");

            foreach (var column in columns)
                if (column != identityColumn)
                    insertColumns.Add($"[{column}]");

            command.Append(string.Join(", ", insertColumns));
            command.Append(") ");

            return command.ToString();
        }

        private static string GetOutputCreateTableCmd(string tmpTablename, string identityColumn)
        {
            return $"CREATE TABLE {tmpTablename}([{identityColumn}] int); ";
        }

        private static string GetSchemaType(this PropertyMapping column, string columnType)
        {
            switch (columnType)
            {
                case "varchar":
                case "nvarchar":
                case "char":
                case "binary":
                case "varbinary":
                case "nchar":
                    if (column.MaxLength != 0)
                        columnType = columnType + $"({column.MaxLength})";
                    break;
                case "decimal":
                case "numeric":
                    columnType = columnType + $"({column.Precision}, {column.Scale})";
                    break;
                case "datetime2":
                case "time":
                    break;
            }

            return columnType;
        }
    }
}