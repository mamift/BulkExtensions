using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Linq;
using BulkExtensions.Shared.Mapping;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Mapping;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class DbContextExtension
    {
        internal static DbContextWrapper GetContextWrapper<TEntity>(this DbContext context) where TEntity : class
        {
            var database = context.Database;
            return new DbContextWrapper(database.Connection, database.CurrentTransaction?.UnderlyingTransaction,
                context.Mapping<TEntity>(), context.Database.CommandTimeout);
        }

        internal static DbContextWrapper GetContextWrapper(this DbContext context, Type type)
        {
            var database = context.Database;
            return new DbContextWrapper(database.Connection, database.CurrentTransaction?.UnderlyingTransaction,
                context.Mapping<object>(type), context.Database.CommandTimeout);
        }

        internal static IEnumerable<IGrouping<Type, EntryWrapper>> GetEntriesByState(this IObjectContextAdapter context,
            EntityState state)
        {
            var objectContext = context.ObjectContext;

            var entries = objectContext.ObjectStateManager
                .GetObjectStateEntries(state)
                .Where(entry => !entry.IsRelationship)
                .Select(entry => new EntryWrapper
                {
                    Entity = entry.Entity,
                    Parent = context.GetRelatedParent(entry.Entity),
                    EntityType = entry.Entity.GetType().BaseType != typeof(object) ? entry.Entity.GetType().BaseType : entry.Entity.GetType()
                })
                .GroupBy(entry => entry.EntityType)
                .ToList();

            return entries;
        }

        private static object GetRelatedParent(this IObjectContextAdapter context, object entity)
        {
            var manager = context.ObjectContext.ObjectStateManager.GetRelationshipManager(entity);
            var relatedEnds = manager.GetAllRelatedEnds();

            foreach (var relatedEnd in relatedEnds)
            {
                dynamic related = manager.GetRelatedEnd(relatedEnd.RelationshipName, relatedEnd.TargetRoleName);
                if (related is EntityReference)
                {
                    return related.Value;
                }
            }

            return null;
        }
    }
}