using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                var entity = entities[index];
                var properties = entity.GetType().GetPropertyInfo().ToList();
                var row = new List<object>();
                foreach (var propertyMapping in propertyMappings)
                {
                    var propertyInfo = properties.SingleOrDefault(info => info.Name == propertyMapping.PropertyName);
                    if (propertyInfo != null && !propertyMapping.IsFk)
                        row.Add(propertyInfo.GetValue(entity, null) ?? DBNull.Value);
                    else if (propertyMapping.IsFk)
                        row.Add(entity.GetForeingKeyValue(properties, propertyInfo, propertyMapping));
                    else if (propertyMapping.IsHierarchyMapping)
                        row.Add(mapping.HierarchyMapping[entity.GetType().Name]);
                    else if (propertyMapping.PropertyName.Equals(SqlHelper.Identity))
                        row.Add(index);
                    else
                        row.Add(DBNull.Value);
                }

                rows.Add(row.ToArray());
            }

            return new EnumerableDataReader(propertyMappings.Select(propertyMapping => propertyMapping.ColumnName), rows);
        }

        private static object GetForeingKeyValue<TEntity>(this TEntity entity, IEnumerable<PropertyInfo> properties,
            PropertyInfo propertyInfo, IPropertyMapping propertyMapping) where TEntity : class
        {
            var navigation = properties
                .Single(info => info.Name == propertyMapping.NavigationProperty.Name)
                .GetValue(entity, null);
            if (navigation == null)
            {
                if (propertyInfo != null)
                    return propertyInfo.GetValue(entity, null) ?? DBNull.Value;
                else
                    return DBNull.Value;
            }
            else
            {
                var detinationProperty = navigation.GetType().GetPropertyInfo()
                    .Single(info => info.Name == propertyMapping.NavigationProperty.PropertyName);

                return detinationProperty.GetValue(navigation, null) ?? DBNull.Value;
            }
        }
    }
}