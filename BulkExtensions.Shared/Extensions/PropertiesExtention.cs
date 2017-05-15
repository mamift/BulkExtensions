using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class PropertiesExtention
    {
        internal static IEnumerable<IPropertyMapping> FilterPropertiesByOperation(this IEnumerable<IPropertyMapping> propertyMappings, Operation operationType)
        {
            switch (operationType)
            {
                case Operation.Insert:
                    return propertyMappings.Where(propertyMapping => !propertyMapping.IsPk).ToList();
                case Operation.Delete:
                    return propertyMappings.Where(propertyMapping => propertyMapping.IsPk).ToList();
                case Operation.Update:
                    return propertyMappings.Where(propertyMapping => !propertyMapping.IsHierarchyMapping).ToList();
                default:
                    return propertyMappings;
            }
        }

        internal static IEnumerable<IPropertyMapping> FilterPropertiesByOptions(this IEnumerable<IPropertyMapping> propertyMappings, BulkOptions options)
        {
            if (!options.HasFlag(BulkOptions.KeepForeingKeys))
            {
                return propertyMappings.Where(property => !property.IsFk);
            }
            return propertyMappings;
        }
    }
}