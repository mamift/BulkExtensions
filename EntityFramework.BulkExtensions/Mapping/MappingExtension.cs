using System;
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
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IEntityMapping Mapping<TEntity>(this DbContext context, Type type = null) where TEntity : class
        {
            var entityTypeMapping = context.GetEntityMapping<TEntity>(type);
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

            var mapping = typeMappings
                .Select(typeMapping => typeMapping.Fragments.First())
                .ToList();
            var navigationProperties = mapping
                .SelectMany(fragment => ((EntityTypeMapping)fragment.TypeMapping).EntityType.NavigationProperties)
                .ToList();
            var scalarPropertyMapping = mapping
                .SelectMany(fragment => fragment.PropertyMappings.OfType<ScalarPropertyMapping>())
                .ToList();

            var keyProperties = mapping
                .Select(fragment => fragment.StoreEntitySet.ElementType)
                .SelectMany(entityType => entityType.KeyProperties)
                .ToList();

            var propertyMappings = new List<IPropertyMapping>();
            scalarPropertyMapping.ForEach(propertyMapping =>
            {
                if (propertyMappings.Any(map => map.ColumnName == propertyMapping.Column.Name)) return;

                propertyMappings.Add(propertyMapping.GetPropertyMapping(keyProperties));

            });

            navigationProperties.ForEach(navigationProperty =>
            {
                navigationProperty.GetNavigationPropertyMappings(propertyMappings, keyProperties);
            });

            return propertyMappings;
        }

        private static void GetNavigationPropertyMappings(this NavigationProperty navigationProperty,
            ICollection<IPropertyMapping> propertyMappings, IEnumerable<EdmMember> keyProperties)
        {
            var parentKeyProperties = navigationProperty.GetDestinationKey().ToList();
            var type = navigationProperty.ToEndMember.DeclaringType as AssociationType;
            var refConst = type?.ReferentialConstraints.ToList();

            if (refConst != null && refConst.Any())
            {
                refConst.ForEach(constraint =>
                {
                    var columnName = constraint.ToProperties.First().Name;
                    var detinationProp = type.ReferentialConstraints.First().FromProperties.First().Name;
                    var propertyMapping = propertyMappings
                            .SingleOrDefault(pMap => pMap.ColumnName.Equals(columnName))
                        as Commons.Mapping.PropertyMapping;

                    if (propertyMapping == null) return;
                    propertyMapping.IsFk = true;
                    propertyMapping.IsPk = keyProperties
                        .Any(prop => prop.Name.Equals(columnName));
                    propertyMapping.NavigationProperty = new NavigationPropertyMapping
                    {
                        Name = navigationProperty.Name,
                        PropertyName = detinationProp
                    };
                });
            }
            else
            {
                parentKeyProperties.ForEach(parentKey =>
                {
                    propertyMappings.Add(new Commons.Mapping.PropertyMapping
                    {
                        ColumnName = $"{navigationProperty.Name}_{parentKey.Name}",
                        IsFk = true,
                        IsPk = keyProperties.Any(prop => prop.Name.Equals($"{navigationProperty.Name}_{parentKey.Name}")),
                        NavigationProperty = new NavigationPropertyMapping
                        {
                            Name = navigationProperty.Name,
                            PropertyName = parentKey.Name
                        }
                    });
                });
            }
        }

        private static IEnumerable<EdmProperty> GetDestinationKey(this NavigationProperty navigationProperty)
        {
            var typeBase = navigationProperty.ToEndMember.DeclaringType as EntityTypeBase;
            var property = typeBase?.KeyMembers.First(member => member.Name.Contains("Target"));
            return ((RefType)property?.TypeUsage?.EdmType)?.ElementType.KeyProperties;
        }

        private static Commons.Mapping.PropertyMapping GetPropertyMapping(this ScalarPropertyMapping propertyMapping,
            IEnumerable<EdmProperty> keyProperties)
        {
            return new Commons.Mapping.PropertyMapping
            {
                ColumnName = propertyMapping.Column.Name,
                PropertyName = propertyMapping.Property.Name,
                IsPk = keyProperties.Any(prop => prop.Name.Equals(propertyMapping.Column.Name)),
                IsDbGenerated = propertyMapping.Column.IsStoreGeneratedIdentity
                                || propertyMapping.Column.IsStoreGeneratedComputed
            };
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
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ReadOnlyCollection<EntityTypeMapping> GetEntityMapping<TEntity>(this IObjectContextAdapter context, Type type = null) where TEntity : class
        {
            var collectionType = type ?? typeof(TEntity);
            var metadata = context.ObjectContext.MetadataWorkspace;
            var objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);
            var entityType = metadata
                .GetItems<EntityType>(DataSpace.OSpace)
                .SingleOrDefault(e => objectItemCollection.GetClrType(e) == collectionType);
            if (entityType == null)
                throw new BulkException(@"Entity is not being mapped by Entity Framework. Verify your EF configuration.");

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