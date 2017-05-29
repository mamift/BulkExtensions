using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public class EntityMapping : IEntityMapping
    {
        public string TableName { get; set; }
        public string Schema { get; set; }
        public IEnumerable<IPropertyMapping> Properties { get; set; }        
        public Dictionary<string, string> HierarchyMapping { get; set; }

        public IEnumerable<IPropertyMapping> Pks
        {
            get { return Properties.Where(propertyMapping => propertyMapping.IsPk); }
        }

        public string FullTableName => string.IsNullOrEmpty(Schema?.Trim())
            ? $"[{TableName}]"
            : $"[{Schema}].[{TableName}]";
        
        public bool HasGeneratedKeys => Properties.Any(property => property.IsPk && property.IsDbGenerated);

        public bool HasComputedColumns => Properties.Any(property => !property.IsPk && property.IsDbGenerated);
    }
}