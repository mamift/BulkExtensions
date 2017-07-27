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
        internal const int NoRowsAffected = 0;

        /// <summary>
        ///
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        internal static string RandomTableName(this IEntityMapping mapping)
        {
            var schema = string.IsNullOrEmpty(mapping.Schema?.Trim())
                ? string.Empty
                : $"[{mapping.Schema}].";
            return $"{schema}[#{mapping.TableName}_{GuidHelper.GetRandomTableGuid()}]";
        }

        /// <summary>
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="tableName"></param>
        /// <param name="operationType"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static string BuildStagingTableCommand(this IEntityMapping mapping, string tableName, Operation operationType,
            BulkOptions options)
        {
            var paramList = mapping
                .GetPropertiesByOperation(operationType)
                .ToList();

            if (paramList.All(s => s.IsPk && s.IsDbGenerated) &&
                operationType == Operation.Update)
                return null;

            var paramColumns = paramList
                .Select(column => $"{Source}.[{column.ColumnName}]")
                .ToList();

            if (mapping.WillOutputGeneratedValues(options))
            {
                paramColumns.Add($"1 as [{Identity}]");
            }

            var paramListConcatenated = string.Join(", ", paramColumns);

            return $"SELECT TOP 0 {paramListConcatenated} INTO {tableName} FROM {mapping.FullTableName} AS A " +
                   $"LEFT JOIN {mapping.FullTableName} AS {Source} ON 1 = 2";
        }

        internal static string BuildMergeCommand(this IDbContextWrapper context, string tmpTableName,
            Operation operationType)
        {
            var command = $"MERGE INTO {context.EntityMapping.FullTableName} WITH (HOLDLOCK) AS {Target} USING {tmpTableName} AS {Source} " +
                $"{context.EntityMapping.PrimaryKeysComparator()} ";

            switch (operationType)
            {
                case Operation.Insert:
                    command += context.EntityMapping.BuildMergeInsertSet();
                    break;
                case Operation.Update:
                    command += context.EntityMapping.BuildMergeUpdateSet();
                    break;
                case Operation.InsertOrUpdate:
                    command += context.EntityMapping.BuildMergeUpdateSet();
                    command += context.EntityMapping.BuildMergeInsertSet();
                    break;
                case Operation.Delete:
                    command += "WHEN MATCHED THEN DELETE";
                    break;
            }

            return command;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="outputTableName"></param>
        /// <param name="propertyMappings"></param>
        /// <param name="items"></param>
        internal static void LoadFromOutputTable<TEntity>(this IDbContextWrapper context, string outputTableName,
            IEnumerable<IPropertyMapping> propertyMappings, IList<TEntity> items)
        {
            var mappings = propertyMappings as IList<IPropertyMapping> ?? propertyMappings.ToList();
            var columnNames = mappings.Select(property => property.ColumnName);
            var command = $"SELECT {Identity}, {string.Join(", ", columnNames)} FROM {outputTableName}";

            using (var reader = context.SqlQuery(command))
            {
                while (reader.Read())
                {
                    var item = items.ElementAt((int)reader[Identity]);

                    foreach (var propertyMapping in mappings)
                    {
                        var propertyInfo = item.GetType().GetProperty(propertyMapping.PropertyName);

                        if (propertyInfo != null && propertyInfo.CanWrite)
                            propertyInfo.SetValue(item, reader[propertyMapping.ColumnName], null);

                        else
                            throw new Exception();
                    }
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
            return $"; DROP TABLE {tableName};";
        }

        /// <summary>
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private static string BuildMergeUpdateSet(this IEntityMapping mapping)
        {
            var command = new StringBuilder();
            var parameters = new List<string>();
            var properties = mapping.Properties
                .Where(propertyMapping => !propertyMapping.IsPk)
                .Where(propertyMapping => !propertyMapping.IsDbGenerated)
                .Where(propertyMapping => !propertyMapping.IsHierarchyMapping)
                .ToList();

            if (properties.Any())
            {
                command.Append("WHEN MATCHED THEN UPDATE SET ");

                foreach (var column in properties)
                {
                    parameters.Add($"[{Target}].[{column.ColumnName}] = [{Source}].[{column.ColumnName}]");
                }

                command.Append(string.Join(", ", parameters) + " ");
            }

            return command.ToString();
        }

        private static string BuildMergeInsertSet(this IEntityMapping mapping)
        {
            var command = new StringBuilder();
            var columns = new List<string>();
            var values = new List<string>();
            var properties = mapping.Properties
                .Where(propertyMapping => !propertyMapping.IsDbGenerated)
                .ToList();

            command.Append(" WHEN NOT MATCHED BY TARGET THEN INSERT ");
            if (properties.Any())
            {
                foreach (var column in properties)
                {
                    columns.Add($"[{column.ColumnName}]");
                    values.Add($"[{Source}].[{column.ColumnName}]");
                }

                command.Append($"({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})");
            }
            else
            {
                command.Append("DEFAULT VALUES");
            }

            return command.ToString();
        }

        internal static string BuildMergeOutputSet(string outputTableName, IEnumerable<IPropertyMapping> properties)
        {
            var propertyMappings = properties as IList<IPropertyMapping> ?? properties.ToList();
            var insertedColumns = string.Join(", ", propertyMappings.Select(property => $"INSERTED.{property.ColumnName}"));
            var outputColumns = string.Join(", ", propertyMappings.Select(property => property.ColumnName));

            return $" OUTPUT {Source}.{Identity}, {insertedColumns} INTO {outputTableName} ({Identity}, {outputColumns})";
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

        internal static string BuildOutputTableCommand(string tmpTablename, IEntityMapping mapping, IEnumerable<IPropertyMapping> propertyMappings)
        {
            return $"SELECT TOP 0 1 as [{Identity}], {string.Join(", ", propertyMappings.Select(property => $"{Source}.[{property.ColumnName}]"))} " +
                   $"INTO {tmpTablename} FROM {mapping.FullTableName} AS A " +
                   $"LEFT JOIN {mapping.FullTableName} AS {Source} ON 1 = 2";
        }
    }
}