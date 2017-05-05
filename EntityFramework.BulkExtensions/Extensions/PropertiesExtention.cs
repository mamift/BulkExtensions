using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.BulkOperations;
using EntityFramework.MappingExtension;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class PropertiesExtention
    {
        internal static IEnumerable<IPropertyMapping> FilterProperties(this IEnumerable<IPropertyMapping> IPropertyMappings, OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Delete:
                    return IPropertyMappings.Where(IPropertyMapping => IPropertyMapping.IsPk).ToList();
                case OperationType.Update:
                    return IPropertyMappings.Where(IPropertyMapping => !IPropertyMapping.IsHierarchyMapping).ToList();
                default:
                    return IPropertyMappings;
            }
        }
    }
}