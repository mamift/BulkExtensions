using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityFramework.BulkExtensions.BulkOperations;

namespace EntityFramework.BulkExtensions.Mapping
{
    internal static class MappingExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        internal static EntityMapping Mapping<TEntity>(this DbContext context, OperationType operation) where TEntity : class
        {
            var entityTypeMapping = context.GetEntityMapping<TEntity>();
            var mappings = entityTypeMapping.Select(typeMapping => typeMapping.Fragments.First()).First();
            var entityType = typeof(TEntity);

            var properties = entityTypeMapping.GetPropertyMapping();
            var entityMapping = new EntityMapping
            {
                TableName = mappings.GetTableName(),
                Schema = mappings.GetTableSchema(),
                EntityName = entityType.Name,
                EntityType = entityType
            };

            if (entityTypeMapping.Any(typeMapping => typeMapping.IsHierarchyMapping) && operation == OperationType.Insert)
            {
                var typeMappings = entityTypeMapping
                    .Where(typeMapping => !typeMapping.IsHierarchyMapping)
                    .ToList();

                entityMapping.HierarchyMapping = GetHierarchyMappings(typeMappings);
                properties.Add(GetDiscriminatorProperty(typeMappings));
            }

            entityMapping.Properties = properties;
            return entityMapping;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        private static PropertyMapping GetDiscriminatorProperty(IEnumerable<EntityTypeMapping> typeMappings)
        {
            var discriminator = typeMappings
                .SelectMany(
                    typeMapping =>
                        typeMapping.Fragments.SelectMany(
                            fragment => fragment.Conditions.OfType<ValueConditionMapping>()))
                .First(conditionMapping => conditionMapping.Property == null);

            return new PropertyMapping
            {
                ColumnName = discriminator.Column.Name,
                DbType = discriminator.Column.TypeName,
                MaxLength = discriminator.Column.MaxLength,
                Type = typeof(string),
                IsHierarchyMapping = true
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetHierarchyMappings(IEnumerable<EntityTypeMapping> typeMappings)
        {
            var hierarchyMapping = new Dictionary<string, string>();
            foreach (var typeMapping in typeMappings)
            {
                var mappingKey = typeMapping.EntityType.Name;
                var mappingValue = typeMapping.Fragments
                    .First().Conditions
                    .OfType<ValueConditionMapping>()
                    .First(conditionMapping => conditionMapping.Property == null)
                    .Value;
                hierarchyMapping.Add(mappingKey, mappingValue.ToString());
            }
            return hierarchyMapping;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityTypeMapping"></param>
        /// <returns></returns>
        private static IList<PropertyMapping> GetPropertyMapping(this IEnumerable<EntityTypeMapping> entityTypeMapping)
        {
            var typeMappings = entityTypeMapping.ToList();
            var mapping = typeMappings.Select(typeMapping => typeMapping.Fragments.First());
            var scalarPropertyMappings = mapping
                .SelectMany(fragment => fragment.PropertyMappings.OfType<ScalarPropertyMapping>())
                .Where(propertyMapping => !propertyMapping.Column.IsStoreGeneratedComputed)
                .ToList();

            var propertyMappings = new List<PropertyMapping>();
            scalarPropertyMappings.ForEach(propertyMapping =>
            {
                if (propertyMappings.All(map => map.ColumnName != propertyMapping.Column.Name))
                {
                    propertyMappings.Add(new PropertyMapping
                    {
                        ColumnName = propertyMapping.Column.Name,
                        DbType = propertyMapping.Column.TypeName,
                        Precision = propertyMapping.Column.Precision,
                        Scale = propertyMapping.Column.Scale,
                        MaxLength = propertyMapping.Column.MaxLength,
                        Type = propertyMapping.Property.UnderlyingPrimitiveType.ClrEquivalentType,
                        PropertyName = propertyMapping.Property.Name,
                        IsPk = ((EntityType)propertyMapping.Column.DeclaringType).KeyProperties
                            .Any(property => property.Name.Equals(propertyMapping.Column.Name))
                    });
                }
            });

            return propertyMappings;
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
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        private static ReadOnlyCollection<EntityTypeMapping> GetEntityMapping<TEntity>(this IObjectContextAdapter context) where TEntity : class
        {
            var metadata = context.ObjectContext.MetadataWorkspace;
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .SingleOrDefault(e => objectItemCollection.GetClrType(e) == typeof(TEntity));
            if (entityType == null)
                throw new EntityException(@"Entity is not being mapped by Entity Framework. Verify your EF configuration.");

            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            return mapping.EntityTypeMappings;
        }
    }
}