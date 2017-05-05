using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.MappingExtension
{
    public class EntityMapping : IEntityMapping
    {
        public Type EntityType { get; set; }
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public string Schema { get; set; }
        public IEnumerable<IPropertyMapping> Properties { get; set; }

        public IEnumerable<IPropertyMapping> Pks
        {
            get { return Properties.Where(propertyMapping => propertyMapping.IsPk); }
        }

        public string FullTableName => $"[{Schema}].[{TableName}]";

        public Dictionary<string, string> HierarchyMapping { get; set; }
    }
}