using System;
using System.Collections.Generic;

namespace EntityFramework.MappingExtension
{
    public interface IEntityMapping
    {
        Type EntityType { get; set; }
        string EntityName { get; set; }
        string TableName { get; set; }
        string Schema { get; set; }
        IEnumerable<IPropertyMapping> Properties { get; set; }

        IEnumerable<IPropertyMapping> Pks { get; }

        string FullTableName { get; }

        Dictionary<string, string> HierarchyMapping { get; set; }
    }
}