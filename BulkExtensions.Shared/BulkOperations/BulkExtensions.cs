using System.Collections.Generic;
#if EF6
using System.Linq;
using System.Data.Entity;
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
            var entries = context.GetChangesToCommit().ToList();

            var currentTranstaction = context.Database.CurrentTransaction;
            var transaction = currentTranstaction ?? context.Database.BeginTransaction();

            foreach (var groupedEntities in entries)
            {
                var toAddOrUpdate = groupedEntities.Where(entry => entry.State.HasFlag(EntryState.Added) || 
                    entry.State.HasFlag(EntryState.Modified));
                context.GetContextWrapper(groupedEntities.Key).CommitTransaction(toAddOrUpdate, Operation.InsertOrUpdate,
                    BulkOptions.OutputIdentity | BulkOptions.OutputComputed);

                var toDelete = groupedEntities.Where(entry => entry.State == EntryState.Deleted);
                context.GetContextWrapper(groupedEntities.Key).CommitTransaction(toDelete, Operation.Delete);
            }

            context.UpdateChangeTrackerState(entries);
            context.SaveChanges();

			if (currentTranstaction == null)
				transaction.Commit();
        }
#endif
    }
}