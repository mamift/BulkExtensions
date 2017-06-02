using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class MappingExtention
    {
        internal static IEnumerable<IPropertyMapping> GetPropertiesByOperation(this IEntityMapping mapping, Operation operationType)
        {
            switch (operationType)
            {
                case Operation.Delete:
                    return mapping.Properties
                        .Where(propertyMapping => propertyMapping.IsPk);
                case Operation.Update:
                    return mapping.Properties
                        .Where(propertyMapping => !propertyMapping.IsHierarchyMapping)
                        .Where(propertyMapping => !propertyMapping.IsDbGenerated);
                default:
                    return mapping.Properties
                        .Where(propertyMapping => propertyMapping.IsPk || !propertyMapping.IsDbGenerated);
            }
        }

        internal static IEnumerable<IPropertyMapping> GetPropertiesByOptions(this IEntityMapping mapping, BulkOptions options)
        {
            if (options.HasFlag(BulkOptions.OutputIdentity) && options.HasFlag(BulkOptions.OutputComputed))
                return mapping.Properties.Where(property => property.IsDbGenerated);
            if (options.HasFlag(BulkOptions.OutputIdentity))
                return mapping.Properties.Where(property => property.IsPk && property.IsDbGenerated);
            if (options.HasFlag(BulkOptions.OutputComputed))
                return mapping.Properties.Where(property => !property.IsPk && property.IsDbGenerated);

            return mapping.Properties;
        }

        internal static bool WillOutputGeneratedValues(this IEntityMapping mapping, BulkOptions options)
        {
            return options.HasFlag(BulkOptions.OutputIdentity) && mapping.HasGeneratedKeys
                   || options.HasFlag(BulkOptions.OutputComputed) && mapping.HasComputedColumns;
        }
    }
}