namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public interface IPropertyMapping
    {
        string PropertyName { get; }
        string ColumnName { get; }
        bool IsPk { get; }
        bool IsFk { get; }
        bool IsHierarchyMapping { get; }
    }
}