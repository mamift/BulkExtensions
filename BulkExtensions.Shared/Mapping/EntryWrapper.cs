using System;

namespace BulkExtensions.Shared.Mapping
{
    public class EntryWrapper
    {
        public object Entity { get; set; }
        public Type EntityType { get; set; }
        public object Parent { get; set; }
    }
}
