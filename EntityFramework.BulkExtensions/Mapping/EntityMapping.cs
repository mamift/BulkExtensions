using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkExtensions.Mapping
{
    internal class EntityMapping
    {
        public Type EntityType { get; set; }
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public string Schema { get; set; }
        public IEnumerable<PropertyMapping> Properties { get; set; }

        public IEnumerable<PropertyMapping> Pks
        {
            get { return Properties.Where(propertyMapping => propertyMapping.IsPk); }
        }

        public string FullTableName => $"[{Schema}].[{TableName}]";

        public Dictionary<string, string> HierarchyMapping { get; set; }
    }
}