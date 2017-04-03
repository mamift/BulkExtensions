using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkExtensions.Metadata
{
    internal class EntityMetadata
    {
        public Type EntityType { get; set; }
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public string Schema { get; set; }
        public IEnumerable<PropertyMetadata> Properties { get; set; }

        public IEnumerable<PropertyMetadata> Pks
        {
            get
            {
                return Properties.Where(metadata => metadata.IsPk);
            }
        }
    }
}