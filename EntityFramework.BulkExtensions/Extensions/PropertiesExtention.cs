using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.BulkOperations;
using EntityFramework.BulkExtensions.Mapping;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class PropertiesExtention
    {
        internal static IEnumerable<PropertyMapping> FilterProperties(this IEnumerable<PropertyMapping> propertyMappings, OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Delete:
                    return propertyMappings.Where(propertyMapping => propertyMapping.IsPk).ToList();
                case OperationType.Update:
                    return propertyMappings.Where(propertyMapping => !propertyMapping.IsHierarchyMapping).ToList();
                default:
                    return propertyMappings;
            }
        }
    }
}