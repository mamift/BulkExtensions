using System;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public interface IPropertyMapping
    {
        Type Type { get; set; }
        string DbType { get; set; }
        string PropertyName { get; set; }
        string ColumnName { get; set; }
        byte? Precision { get; set; }
        byte? Scale { get; set; }
        int? MaxLength { get; set; }
        bool IsPk { get; set; }
        bool IsHierarchyMapping { get; set; }
    }
}