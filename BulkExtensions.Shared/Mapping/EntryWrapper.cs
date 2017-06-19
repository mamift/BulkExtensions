using System;
using System.Collections.Generic;
using EntityFramework.BulkExtensions.Commons.Flags;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    public class EntryWrapper
    {
        public object Entity { get; set; }
        public Type EntitySetType { get; set; }
        public IDictionary<string, object> ForeignKeys { get; set; }
        public EntryState State { get; set; }
    }
}
