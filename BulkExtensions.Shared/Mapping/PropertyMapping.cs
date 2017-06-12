namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public class PropertyMapping : IPropertyMapping
    {
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPk { get; set; }
        public bool IsFk { get; set; }
        public bool IsHierarchyMapping { get; set; }
        public bool IsDbGenerated { get; set; }
        public NavigationPropertyMapping NavigationProperty { get; set; }
    }

    public class NavigationPropertyMapping
    {
        public string Name { get; set; }
        public string PropertyName { get; set; }
    }
}