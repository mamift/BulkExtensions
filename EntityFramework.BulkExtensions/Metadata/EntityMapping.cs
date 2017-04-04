using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace EntityFramework.BulkExtensions.Metadata
{
    internal static class EntityMapping
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static EntityMetadata Metadata<T>(this DbContext context)
        {
            var entityTypeMapping = context.GetEntityMapping<T>();
            var mapping = entityTypeMapping.Fragments.Single();
            var propertyType = typeof(T);

            return new EntityMetadata
            {
                TableName = mapping.GetTableName(),
                Schema = mapping.GetTableSchema(),
                EntityName = propertyType.Name,
                EntityType = propertyType,
                Properties = entityTypeMapping.GetPropertyMetadata()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityTypeMapping"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyMetadata> GetPropertyMetadata(this EntityTypeMapping entityTypeMapping)
        {
            var mapping = entityTypeMapping.Fragments.Single();
            var scalarPropertyMappings = mapping.PropertyMappings.OfType<ScalarPropertyMapping>();
            return scalarPropertyMappings.Select(propertyMapping => new PropertyMetadata
            {
                ColumnName = propertyMapping.Column.Name,
                DbType = propertyMapping.Column.TypeName,
                Precision = propertyMapping.Column.Precision,
                Scale = propertyMapping.Column.Scale,
                MaxLength = propertyMapping.Column.MaxLength,
                Type = propertyMapping.Property.UnderlyingPrimitiveType.ClrEquivalentType,
                PropertyName = propertyMapping.Property.Name,
                IsPk = entityTypeMapping.EntityType.KeyProperties.Any(property => property.Name == propertyMapping.Column.Name)
            }).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private static string GetTableName(this MappingFragment mapping)
        {
            var entitySet = mapping.StoreEntitySet;
            return (string)entitySet.MetadataProperties["Table"].Value ?? entitySet.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private static string GetTableSchema(this MappingFragment mapping)
        {
            var entitySet = mapping.StoreEntitySet;
            return (string)entitySet.MetadataProperties["Schema"].Value ?? entitySet.Schema;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        private static EntityTypeMapping GetEntityMapping<T>(this DbContext context)
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(e => objectItemCollection.GetClrType(e) == typeof(T));
            if (entityType == null)
                throw new EntityException(@"Entity is not being mapped by Entity Framework. Check your model.");

            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            return mapping
                .EntityTypeMappings.Single();
        }
    }
}