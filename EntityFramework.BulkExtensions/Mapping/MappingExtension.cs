using System;
using System.Collections.Generic;
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
            var entitySetMapping = context.GetEntityMapping<TEntity>(type);
            var entityTypeMapping = entitySetMapping.EntityTypeMappings;
            var mappings = entityTypeMapping.Select(typeMapping => typeMapping.Fragments.First()).First();

            var entityMapping = new EntityMapping
            {
                TableName = mappings.GetTableName(),
                Schema = mappings.GetTableSchema()
            };

            var properties = entitySetMapping.GetIPropertyMapping(entityMapping);

            if (entityTypeMapping.Any(typeMapping => typeMapping.IsHierarchyMapping))
            {
                var typeMappings = entityTypeMapping
                    .Where(typeMapping => !typeMapping.IsHierarchyMapping)
                    .ToList();

                entityMapping.HierarchyMapping = GetHierarchyMappings(typeMappings);
                var discriminator = GetDiscriminatorProperty(typeMappings);
                if (!properties.Any(mapping => mapping.ColumnName.Equals(discriminator.ColumnName)))
                {
                    properties.Add(discriminator);
                }
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
        /// <returns></returns>
        private static IList<IPropertyMapping> GetIPropertyMapping(this EntitySetMapping entitySetMapping,
            EntityMapping entityMapping)
        {
            var typeMappings = entitySetMapping.EntityTypeMappings.ToList();

            var mapping = typeMappings
                .Select(typeMapping => typeMapping.Fragments.First())
                .ToList();

            var scalarPropertyMapping = mapping
                .SelectMany(fragment => fragment.PropertyMappings.OfType<ScalarPropertyMapping>())
                .ToList();

            var navigationProperties = mapping
                .Where(fragment => ((EntityTypeMapping)fragment.TypeMapping).EntityType != null)
                .SelectMany(fragment => ((EntityTypeMapping)fragment.TypeMapping).EntityType.NavigationProperties)
                .Distinct()
                .ToList();

            var keyProperties = mapping
                .Select(fragment => fragment.StoreEntitySet.ElementType)
                .SelectMany(entityType => entityType.KeyProperties)
                .ToList();

            var propertyMappings = new List<IPropertyMapping>();

            propertyMappings.AddRange(entitySetMapping.GetAssociationForeignKeys(entityMapping, keyProperties));

            scalarPropertyMapping.ForEach(propertyMapping =>
            {
                if (propertyMappings.Any(map => map.ColumnName == propertyMapping.Column.Name)) return;

                propertyMappings.Add(propertyMapping.GetPropertyMapping(keyProperties));

            });

            navigationProperties.ForEach(navigationProperty =>
            {
                if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many) return;
                navigationProperty.GetNavigationForeignKeys(propertyMappings, keyProperties);
            });


            return propertyMappings;
        }

        private static Commons.Mapping.PropertyMapping GetPropertyMapping(this ScalarPropertyMapping propertyMapping,
            IList<EdmProperty> keyProperties)
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
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<IPropertyMapping> GetAssociationForeignKeys(this EntitySetMapping entitySetMapping,
            EntityMapping entityMapping, IList<EdmProperty> keyProperties)
        {
            var associations = entitySetMapping.ContainerMapping.AssociationSetMappings
                .Where(association => association.StoreEntitySet.Name == entityMapping.TableName
                                      && association.StoreEntitySet.Schema == entityMapping.Schema)
                .ToList();

            var foreingKeys = new List<IPropertyMapping>();
            foreach (var association in associations)
            {
                foreach (var propertyMapping in association.SourceEndMapping.PropertyMappings)
                {
                    foreingKeys.Add(new Commons.Mapping.PropertyMapping
                    {
                        ColumnName = propertyMapping.Column.Name,
                        PropertyName = propertyMapping.Property.Name,
                        IsFk = true,
                        IsHierarchyMapping = false,
                        IsPk = keyProperties
                            .Any(prop => prop.Name.Equals(propertyMapping.Column.Name)),
                        IsDbGenerated = propertyMapping.Column.IsStoreGeneratedIdentity
                                        || propertyMapping.Column.IsStoreGeneratedComputed,
                        ForeignKeyName = $"{association.AssociationSet.Name}_{propertyMapping.Property.Name}"

                    });
                }
            }

            return foreingKeys;
        }

        private static void GetNavigationForeignKeys(this NavigationProperty navigationProperty,
            ICollection<IPropertyMapping> propertyMappings, IEnumerable<EdmProperty> keyProperties)
        {
            var association = navigationProperty.ToEndMember.DeclaringType as AssociationType;
            var refConst = association?.ReferentialConstraints.ToList();

            if (refConst != null && refConst.Any())
            {
                refConst.ForEach(constraint =>
                {
                    var columnName = constraint.ToProperties.First().Name;
                    var detinationProp = association.ReferentialConstraints.First().FromProperties.First().Name;
                    var propertyMapping = propertyMappings
                            .SingleOrDefault(pMap => pMap.ColumnName.Equals(columnName))
                        as Commons.Mapping.PropertyMapping;

                    if (propertyMapping == null) return;
                    propertyMapping.IsFk = true;
                    propertyMapping.IsPk = keyProperties
                        .Any(prop => prop.Name.Equals(columnName));
                    propertyMapping.ForeignKeyName = $"{association.Name}_{detinationProp}";
                });
            }
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
        private static EntitySetMapping GetEntityMapping<TEntity>(this IObjectContextAdapter context, Type type = null) where TEntity : class
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
                .Single(s => s.ElementType.Name == entityType.GetSetType().Name);

            return metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                .Single()
                .EntitySetMappings
                .Single(s => s.EntitySet == entitySet);
        }

        private static EdmType GetSetType(this EdmType entityType)
        {
            return entityType.BaseType == null ? entityType : entityType.BaseType.GetSetType();
        }
    }
}