using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class DataReaderExtension
    {
        internal static EnumerableDataReader ToDataReader<TEntity>(this IList<TEntity> entities, IEntityMapping mapping,
            IEnumerable<IPropertyMapping> tableColumns) where TEntity : class
        {
            var rows = new ConcurrentBag<object[]>();

            Parallel.ForEach(entities, (item, state, index) =>
            {
                var props = item.GetType()
                    .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                var row = new List<object>();
                foreach (var column in tableColumns)
                {
                    var prop = props.SingleOrDefault(info => info.Name == column.PropertyName);
                    if (prop != null)
                        row.Add(prop.GetValue(item, null) ?? DBNull.Value);
                    else if (column.IsHierarchyMapping)
                        row.Add(mapping.HierarchyMapping[item.GetType().Name]);
                    else if (column.PropertyName.Equals(SqlHelper.Identity))
                        row.Add(index);
                    else
                        row.Add(DBNull.Value);
                }

                rows.Add(row.ToArray());
            });

            return new EnumerableDataReader(tableColumns.Select(propertyMapping => propertyMapping.ColumnName), rows);
        }
    }
}