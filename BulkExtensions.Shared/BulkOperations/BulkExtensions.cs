using System.Collections.Generic;
#if EF6
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#endif
using EntityFramework.BulkExtensions.Commons.BulkOperations;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Extensions;
#if !EF6
using Microsoft.EntityFrameworkCore;
#endif

namespace EntityFramework.BulkExtensions
{
    public static class BulkExtensions
    {
        /// <summary>
        /// Bulk insert a collection of objects into the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be inserted.</param>
        /// <param name="options"></param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkInsert<TEntity>(this DbContext context, IEnumerable<TEntity> entities, InsertOptions options = InsertOptions.Default) where TEntity : class
        {
            return context.GetContextWrapper<TEntity>().CommitTransaction(entities, Operation.Insert, options.ToSharedOptions());
        }

        /// <summary>
        /// Bulk update a collection of objects into the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be updated.</param>
        /// <param name="options"></param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities, UpdateOptions options = UpdateOptions.Default) where TEntity : class
        {
            return context.GetContextWrapper<TEntity>().CommitTransaction(entities, Operation.Update, options.ToSharedOptions());
        }

        /// <summary>
        /// Bulk update a collection of objects into the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be updated.</param>
        /// <param name="options"></param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkInsertOrUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities, InsertOptions options = InsertOptions.Default) where TEntity : class
        {
            return context.GetContextWrapper<TEntity>().CommitTransaction(entities, Operation.InsertOrUpdate, options.ToSharedOptions());
        }

        /// <summary>
        /// Bulk delete a collection of objects from the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be deleted.</param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkDelete<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class
        {
            return context.GetContextWrapper<TEntity>().CommitTransaction(entities, Operation.Delete);
        }

#if EF6

        public static void BulkSaveChanges(this DbContext context)
        {
            var toAddOrUpdate = context.GetEntriesByState(EntityState.Added | EntityState.Modified).ToList();
            var toDelete = context.GetEntriesByState(EntityState.Deleted);

            var currentTranstaction = context.Database.CurrentTransaction;
            using (var transaction = currentTranstaction ?? context.Database.BeginTransaction())
            {
                foreach (var groupedEntities in toAddOrUpdate)
                {
                    var entities = groupedEntities
                        .Select(entry => entry.Entity)
                        .ToList();
                    context.GetContextWrapper(groupedEntities.Key).CommitTransaction(entities, Operation.InsertOrUpdate,
                        BulkOptions.OutputIdentity | BulkOptions.OutputComputed);
                }

                foreach (var groupedEntities in toDelete)
                {
                    var entities = groupedEntities
                        .Select(entry => entry.Entity)
                        .ToList();
                    context.GetContextWrapper(groupedEntities.Key).CommitTransaction(entities, Operation.Delete);
                }

                if (currentTranstaction == null)
                    transaction.Commit();
            }

            foreach (var groupedEntity in toAddOrUpdate.SelectMany(entries => entries))
            {
                context.Set(groupedEntity.Entity.GetType()).Attach(groupedEntity.Entity);
                var manager =
                    ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager
                    .GetRelationshipManager(groupedEntity.Entity);

            }
            var RelationshipObjects =
                ((IObjectContextAdapter) context).ObjectContext.ObjectStateManager.GetObjectStateEntries(EntityState
                    .Added | EntityState.Modified);
            foreach (var objectStateEntry in RelationshipObjects)
            {
                objectStateEntry.AcceptChanges();
            }
        }
#endif
    }
}