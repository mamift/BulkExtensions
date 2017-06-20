using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Exceptions;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Mapping
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
            var entityTypeMapping = context.GetEntityMapping<TEntity>();
            var mappings = entityTypeMapping.Select(typeMapping => typeMapping.Fragments.First()).First();
            var properties = entityTypeMapping.GetIPropertyMapping();

            var entityMapping = new EntityMapping
            {
                TableName = mappings.GetTableName(),
                Schema = mappings.GetTableSchema()
            };

            if (entityTypeMapping.Any(typeMapping => typeMapping.IsHierarchyMapping))
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
        private static IPropertyMapping GetDiscriminatorProperty(IEnumerable<EntityTypeMapping> typeMappings)
        {
            var discriminator = typeMappings
                .SelectMany(
                    typeMapping =>
                        typeMapping.Fragments.SelectMany(
                            fragment => fragment.Conditions.OfType<ValueConditionMapping>()))
                .First(conditionMapping => conditionMapping.Property == null);

            return new Commons.Mapping.PropertyMapping
            {
                ColumnName = discriminator.Column.Name,
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
        /// </summary>
        /// <param name="entityTypeMapping"></param>
        /// <returns></returns>
        private static IList<IPropertyMapping> GetIPropertyMapping(this IEnumerable<EntityTypeMapping> entityTypeMapping)
        {
            var typeMappings = entityTypeMapping.ToList();
            var mapping = typeMappings.Select(typeMapping => typeMapping.Fragments.First());
            var scalarIPropertyMappings = mapping
                .SelectMany(fragment => fragment.PropertyMappings.OfType<ScalarPropertyMapping>())
                .ToList();

            var propertyMappings = new List<IPropertyMapping>();
            scalarIPropertyMappings.ForEach(propertyMapping =>
            {
                if (propertyMappings.Any(map => map.ColumnName == propertyMapping.Column.Name)) return;

                propertyMappings.Add(new Commons.Mapping.PropertyMapping
                {
                    ColumnName = propertyMapping.Column.Name,
                    PropertyName = propertyMapping.Property.Name,
                    IsPk = ((EntityType)propertyMapping.Column.DeclaringType).KeyProperties
                        .Any(property => property.Name.Equals(propertyMapping.Column.Name)),
                    IsDbGenerated = propertyMapping.Column.IsStoreGeneratedIdentity
                    || propertyMapping.Column.IsStoreGeneratedComputed
                });

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
            var objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);
            var entityType = metadata
                .GetItems<EntityType>(DataSpace.OSpace)
                .SingleOrDefault(e => objectItemCollection.GetClrType(e) == typeof(TEntity));
            if (entityType == null)
                throw new BulkException(@"Entity is not being mapped by Entity Framework. Verify your EF configuration.");

            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.GetSetType().Name);

            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                .Single()
                .EntitySetMappings
                .Single(s => s.EntitySet == entitySet);

            return mapping.EntityTypeMappings;
        }

        private static EdmType GetSetType(this EdmType entityType)
        {
            return entityType.BaseType == null ? entityType : entityType.BaseType.GetSetType();
        }
    }
}