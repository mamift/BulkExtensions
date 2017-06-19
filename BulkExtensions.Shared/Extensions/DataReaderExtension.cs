using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class DataReaderExtension
    {
        internal static EnumerableDataReader ToDataReader<TEntity>(this IList<TEntity> entities, IEntityMapping mapping,
            IEnumerable<IPropertyMapping> tableColumns) where TEntity : class
        {
            var rows = new List<object[]>();

            var propertyMappings = tableColumns as IList<IPropertyMapping> ?? tableColumns.ToList();
            for (var index = 0; index < entities.Count; index++)
            {
                var obj = entities[index];
                var wrapper = obj as EntryWrapper;
                var entity = wrapper != null
                    ? wrapper.Entity
                    : obj;

                var entityType = entity.GetType();
                var properties = entityType.GetPropertyInfo().ToList();
                var row = new List<object>();
                foreach (var propertyMapping in propertyMappings)
                {
                    var propertyInfo = properties.SingleOrDefault(info => info.Name == propertyMapping.PropertyName);
                    if (propertyInfo != null && !propertyMapping.IsFk)
                        row.Add(propertyInfo.GetValue(entity, null) ?? DBNull.Value);
                    else if (propertyMapping.IsFk && wrapper != null)
                        row.Add(wrapper.GetForeingKeyValue(propertyMapping));
					else if (propertyMapping.IsFk && propertyInfo != null)
						row.Add(propertyInfo.GetValue(entity, null) ?? DBNull.Value);
                    else if (propertyMapping.IsHierarchyMapping)
                        row.Add(mapping.HierarchyMapping[entityType.Name]);
                    else if (propertyMapping.PropertyName.Equals(SqlHelper.Identity))
                        row.Add(index);
                    else
                        row.Add(DBNull.Value);
                }

                rows.Add(row.ToArray());
            }

            return new EnumerableDataReader(propertyMappings.Select(propertyMapping => propertyMapping.ColumnName), rows);
        }

        private static object GetForeingKeyValue(this EntryWrapper wrapper, IPropertyMapping propertyMapping)
        {
            if(wrapper?.ForeignKeys == null)
                return DBNull.Value;
            if (wrapper.ForeignKeys.TryGetValue(propertyMapping.ForeignKeyName, out object value))
                return value;
            return DBNull.Value;
        }
    }
}