using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using EntityFramework.BulkExtensions.Metadata;

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
        /// <param name="metadata"></param>
        /// <returns></returns>
        internal static string RandomTableName(this EntityMetadata metadata)
        {
            return $"[{metadata.Schema}].[_{metadata.TableName}_{GuidHelper.GetRandomTableGuid()}]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="tableName"></param>
        /// <param name="primaryKeysOnly"></param>
        /// <returns></returns>
        internal static string CreateTempTable(this EntityMetadata metadata, string tableName, bool primaryKeysOnly = false)
        {
            var columns = primaryKeysOnly ? metadata.Pks : metadata.Properties;
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
        /// <param name="metadata"></param>
        /// <returns></returns>
        internal static string BuildUpdateSet(this EntityMetadata metadata)
        {
            var command = new StringBuilder();
            var parameters = new List<string>();

            command.Append("SET ");

            foreach (var column in metadata.Properties)
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
        /// <param name="metadata"></param>
        /// <returns></returns>
        internal static string PrimaryKeysComparator(this EntityMetadata metadata)
        {
            var updateOn = metadata.Pks.ToList();
            var command = new StringBuilder();

            command.Append($"ON [{Target}].[{updateOn.First().ColumnName}] = [{Source}].[{updateOn.First().ColumnName}] ");

            if (updateOn.Count > 1)
                foreach (var key in updateOn.Skip(1))
                    command.Append($"AND [{Target}].[{key.ColumnName}] = [{Source}].[{key.ColumnName}]");

            return command.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="tmpOutputTableName"></param>
        /// <param name="tmpTableName"></param>
        /// <param name="identityColumn"></param>
        /// <returns></returns>
        internal static string GetInsertIntoStagingTableCmd(this EntityMetadata metadata, string tmpOutputTableName,
            string tmpTableName, string identityColumn)
        {
            var columns = metadata.Properties.Select(propertyMetadata => propertyMetadata.ColumnName).ToList();

            var comm = GetOutputCreateTableCmd(tmpOutputTableName, identityColumn)
                       + BuildInsertIntoSet(columns, identityColumn, metadata.FullTableName)
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
        /// <param name="identityColumn"></param>
        /// <param name="items"></param>
        internal static void LoadFromTmpOutputTable<TEntity>(this Database context, string tmpOutputTableName,
            string identityColumn, IList<TEntity> items)
        {
            var command = $"SELECT {identityColumn} FROM {tmpOutputTableName} ORDER BY {identityColumn};";
            var identities = context.SqlQuery<int>(command);
            var counter = 0;

            foreach (var result in identities)
            {
                var property = items[counter].GetType().GetProperty(identityColumn);

                if (property.CanWrite)
                    property.SetValue(items[counter], result, null);

                else
                    throw new Exception();

                counter++;
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

        private static string GetSchemaType(this PropertyMetadata column, string columnType)
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