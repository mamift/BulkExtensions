using System;
using System.Collections.Generic;
using System.Linq;
#if NETSTANDARD1_3
using System.Reflection;
#endif
using System.Text;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Helpers
{
    internal static class SqlHelper
    {
        private const string Source = "Source";
        private const string Target = "Target";
        internal const string Identity = "Bulk_Identity";

        /// <summary>
        ///
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        internal static string RandomTableName(this IEntityMapping mapping)
        {
            return $"[_{mapping.TableName}_{GuidHelper.GetRandomTableGuid()}]";
        }

        /// <summary>
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="tableName"></param>
        /// <param name="operationType"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static string CreateTempTable(this IEntityMapping mapping, string tableName, Operation operationType,
            BulkOptions options)
        {
            var paramList = mapping.Properties
                .FilterPropertiesByOperation(operationType)
                .Select(column => $"[{Source}.{column.ColumnName}]")
                .ToList();

            if (operationType == Operation.InsertOrUpdate && options.HasFlag(BulkOptions.OutputIdentity))
            {
                paramList.Add($"1 as [{Identity}]");
            }

            var paramListConcatenated = string.Join(", ", paramList);

            return $"SELECT TOP 0 {paramListConcatenated} INTO {tableName} FROM {mapping.TableName} AS A " +
                   $"LEFT JOIN {mapping.TableName} AS {Source} ON 1 = 2";
        }

        internal static string BuildMergeCommand(this IDbContextWrapper context, string tmpTableName,
            Operation operationType)
        {
            var command =
                $"MERGE INTO {context.EntityMapping.FullTableName} WITH (HOLDLOCK) AS {Target} USING {tmpTableName} AS {Source} " +
                $"{context.EntityMapping.PrimaryKeysComparator()} ";

            switch (operationType)
            {
                case Operation.Update:
                    command += context.EntityMapping.BuildMergeUpdateSet();
                    command += $";{GetDropTableCommand(tmpTableName)}";
                    break;
                case Operation.InsertOrUpdate:
                    command += context.EntityMapping.BuildMergeUpdateSet();
                    command += operationType == Operation.InsertOrUpdate
                        ? context.EntityMapping.BuildMergeInsertSet()
                        : string.Empty;
                    break;
                case Operation.Delete:
                    command += "WHEN MATCHED THEN DELETE";
                    command += $";{GetDropTableCommand(tmpTableName)}";
                    break;
            }

            return command;
        }

        /// <summary>
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="outputTableName"></param>
        /// <param name="tmpTableName"></param>
        /// <param name="identityColumn"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        internal static string GetInsertIntoStagingTableCmd(this IEntityMapping mapping, string outputTableName,
            string tmpTableName, string identityColumn, Operation operationType)
        {
            var columns = mapping.Properties
                .FilterPropertiesByOperation(operationType)
                .Select(propertyMapping => propertyMapping.ColumnName)
                .ToList();

            var comm = CreateOutputTableCmd(outputTableName, identityColumn, operationType)
                       + BuildInsertIntoSet(columns, identityColumn, mapping.FullTableName)
                       + $"OUTPUT INSERTED.{identityColumn} INTO "
                       + outputTableName + $"([{identityColumn}]) "
                       + BuildSelectSet(columns, identityColumn)
                       + $" FROM {tmpTableName} AS {Source}; "
                       + GetDropTableCommand(tmpTableName);

            return comm;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="outputTableName"></param>
        /// <param name="propertyMapping"></param>
        /// <param name="items"></param>
        /// <param name="operation"></param>
        internal static void LoadFromTmpOutputTable<TEntity>(this IDbContextWrapper context, string outputTableName,
            IPropertyMapping propertyMapping, IList<TEntity> items, Operation operation)
        {
            var command = operation == Operation.Insert
                ? $"SELECT {propertyMapping.ColumnName} FROM {outputTableName} ORDER BY {propertyMapping.ColumnName};"
                : $"SELECT {Identity}, {propertyMapping.ColumnName} FROM {outputTableName}";

            if (operation == Operation.InsertOrUpdate)
            {
                using (var reader = context.SqlQuery(command))
                {
                    while (reader.Read())
                    {
                        var item = items.ElementAt((int) reader[0]);

                        var property = item.GetType().GetProperty(propertyMapping.PropertyName);

                        if (property != null && property.CanWrite)
                            property.SetValue(item, reader[1], null);

                        else
                            throw new Exception();
                    }
                }
            }

            else if (operation == Operation.Insert)
            {
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
            }

            command = GetDropTableCommand(outputTableName);
            context.ExecuteSqlCommand(command);
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
        private static string BuildMergeUpdateSet(this IEntityMapping mapping)
        {
            var command = new StringBuilder();
            var parameters = new List<string>();

            command.Append("WHEN MATCHED THEN UPDATE SET ");

            foreach (var column in mapping.Properties)
            {
                if (column.IsPk || column.IsHierarchyMapping) continue;

                parameters.Add($"[{Target}].[{column.ColumnName}] = [{Source}].[{column.ColumnName}]");
            }

            command.Append(string.Join(", ", parameters) + " ");

            return command.ToString();
        }

        private static string BuildMergeInsertSet(this IEntityMapping mapping)
        {
            var command = new StringBuilder();
            var columns = new List<string>();
            var values = new List<string>();

            command.Append(" WHEN NOT MATCHED BY TARGET THEN INSERT (");

            foreach (var column in mapping.Properties)
            {
                if (!column.IsPk)
                {
                    columns.Add($"[{column.ColumnName}]");
                    values.Add($"[{Source}].[{column.ColumnName}]");
                }
            }

            command.Append(string.Join(", ", columns));
            command.Append(") VALUES (");
            command.Append(string.Join(", ", values));
            command.Append(")");

            return command.ToString();
        }

        internal static string BuildOutputId(string outputTableName, string identityColumn)
        {
            return
                $"OUTPUT {Source}.{Identity}, INSERTED.{identityColumn} INTO {outputTableName} ({Identity}, {identityColumn})";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private static string PrimaryKeysComparator(this IEntityMapping mapping)
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

        private static string BuildSelectSet(IEnumerable<string> columns, string identityColumn)
        {
            var command = new StringBuilder();
            var selectColumns = new List<string>();

            command.Append("SELECT ");

            foreach (var column in columns)
            {
                if ((identityColumn == null || column == identityColumn) && identityColumn != null) continue;
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

        internal static string CreateOutputTableCmd(string tmpTablename, string identityColumn, Operation operationType)
        {
            return operationType == Operation.InsertOrUpdate
                ? $"CREATE TABLE {tmpTablename} ([{Identity}] int, [{identityColumn}] int)"
                : $"CREATE TABLE {tmpTablename} ([{identityColumn}] int); ";
        }
    }
}