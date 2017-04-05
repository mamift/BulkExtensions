using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using EntityFramework.BulkExtensions.Metadata;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class DataTableExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="entities"></param>
        /// <param name="primaryKeysOnly"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        internal static DataTable ToDataTable<TEntity>(this IEnumerable<TEntity> entities, EntityMetadata metadata, bool primaryKeysOnly = false) where TEntity : class
        {
            var tb = CreateDataTable(metadata, primaryKeysOnly);
            var tableColumns = primaryKeysOnly ? metadata.Pks.ToList() : metadata.Properties.ToList();

            foreach (var item in entities)
            {
                var props = item.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                var values = new List<object>();
                foreach (var column in tableColumns)
                {
                    var prop = props.SingleOrDefault(info => info.Name == column.PropertyName);
                    if (prop != null)
                        values.Add(prop.GetValue(item, null));
                    else if (column.IsHierarchyMapping)
                        values.Add(metadata.HierarchyMapping[item.GetType().Name]);
                    else
                        values.Add(null);
                }

                tb.Rows.Add(values.ToArray());
            }

            return tb;
        }

        private static DataTable CreateDataTable(EntityMetadata metadata, bool primaryKeysOnly = false)
        {
            var table = new DataTable();
            var columns = primaryKeysOnly ? metadata.Pks : metadata.Properties;
            foreach (var prop in columns)
            {
                table.Columns.Add(prop.ColumnName, Nullable.GetUnderlyingType(prop.Type) ?? prop.Type);
            }

            table.TableName = metadata.EntityName;
            return table;
        }
    }
}