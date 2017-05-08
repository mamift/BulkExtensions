using System.Collections.Generic;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.BulkExtensions.Mapping
{
    internal static class MappingExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static IEntityMapping Mapping<TEntity>(this DbContext context) where TEntity : class
        {
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var entityMapping = new EntityMapping
            {
                EntityName = entityType.Name,
                EntityType = typeof(TEntity),
                TableName = entityType.Relational().TableName,
                Schema = entityType.Relational().Schema,
                Properties = entityType.GetPropertyMappings()
            };

            return entityMapping;
        }

        private static IEnumerable<IPropertyMapping> GetPropertyMappings(this IEntityType entityType)
        {
            return entityType.GetProperties()
                .Select(property => new PropertyMapping
                {
                    PropertyName = property.Name,
                    ColumnName = property.Relational().ColumnName,
                    DbType = property.Relational().ColumnType,
                    IsPk = property.IsPrimaryKey(),
                    MaxLength = property.GetMaxLength(),
                    Type = property.ClrType
                });
        }
    }
}