using System;

namespace EntityFramework.MappingExtension
{
    public class PropertyMapping : IPropertyMapping
    {
        public Type Type { get; set; }
        public string DbType { get; set; }
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }
        public int? MaxLength { get; set; }
        public bool IsPk { get; set; }
        public bool IsHierarchyMapping { get; set; }
    }
}