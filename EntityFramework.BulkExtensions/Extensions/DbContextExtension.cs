using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Mapping;
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

		internal static IEnumerable<IGrouping<Type, EntryWrapper>> GetChangesToCommit(this DbContext context)
		{
			context.ChangeTracker.DetectChanges();
			var objectContext = ((IObjectContextAdapter)context).ObjectContext;

			var enumerable = objectContext.ObjectStateManager
				.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted);
			var entries = enumerable
				.Where(entry => !entry.IsRelationship)
				.Select(entry => new EntryWrapper
				{
					Entity = entry.Entity,
					ForeignKeys = context.GetForeignKeysMap(entry),
					EntitySetType = entry.GetClrType(),
					State = entry.GetEntryState()
				})
				.GroupBy(entry => entry.EntitySetType);

			return entries;
		}

		internal static void UpdateChangeTrackerState(this DbContext context,
			IEnumerable<IGrouping<Type, EntryWrapper>> entries)
		{
			foreach (var wrapper in entries.SelectMany(entryGroup => entryGroup.Select(entry => entry)))
			{
				if (wrapper.State.HasFlag(EntryState.Added | EntryState.Modified))
					context.Set(wrapper.EntitySetType).Attach(wrapper.Entity);
				else if (wrapper.State.HasFlag(EntryState.Deleted))
					context.Entry(wrapper.Entity).State = EntityState.Detached;
			}

			var relationshipObjects = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager
				.GetObjectStateEntries(EntityState.Added)
				.Where(entry => entry.IsRelationship);

			foreach (var objectStateEntry in relationshipObjects)
			{
				objectStateEntry.AcceptChanges();
			}
		}

		private static IDictionary<string, object> GetForeignKeysMap(this IObjectContextAdapter context, ObjectStateEntry entry)
		{
			var manager = context.ObjectContext.ObjectStateManager.GetRelationshipManager(entry.Entity);
			var relatedEnds = manager.GetAllRelatedEnds();

			var foreignKeys = new Dictionary<string, object>();
			foreach (var relatedEnd in relatedEnds)
			{
				if (relatedEnd is EntityReference related)
				{
					var entityKeyValues = related.EntityKey?.EntityKeyValues;
					if (entityKeyValues != null)
						entityKeyValues.ToList().ForEach(foreignKey =>
						{
							foreignKeys[$"{related.RelationshipSet.Name}_{foreignKey.Key}"] = foreignKey.Value;
						});
				}
			}

			return foreignKeys;
		}

		private static EntryState GetEntryState(this ObjectStateEntry entry)
		{
			switch (entry.State)
			{
				case EntityState.Added:
					return EntryState.Added;
				case EntityState.Deleted:
					return EntryState.Deleted;
				case EntityState.Modified:
					return EntryState.Modified;
				default:
					return EntryState.Unchanged;
			}
		}

		private static Type GetClrType(this ObjectStateEntry entry)
		{
			return entry.EntitySet.ElementType.MetadataProperties
				.Single(metadata => metadata.Name.Contains("ClrType"))
				.Value as Type;
		}
	}
}