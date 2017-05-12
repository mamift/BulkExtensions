using System.Collections.Generic;
using System.Data.Entity;
using EntityFramework.BulkExtensions.Commons.BulkOperations;
using EntityFramework.BulkExtensions.Extensions;

namespace EntityFramework.BulkExtensions
{
    public static class BulkExtensions
    {
        /// <summary>
        /// Bulk insert a collection of objects into the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be inserted.</param>
        /// <param name="option"></param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkInsert<TEntity>(this DbContext context, IEnumerable<TEntity> entities, InsertOptions options = InsertOptions.Default) where TEntity : class
        {
            return OperationFactory.BulkInsert.CommitTransaction(context.GetContextWrapper<TEntity>(), entities, options.ToSharedOptions());
        }

        /// <summary>
        /// Bulk update a collection of objects into the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be updated.</param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class
        {
            return OperationFactory.BulkUpdate.CommitTransaction(context.GetContextWrapper<TEntity>(), entities);
        }

        /// <summary>
        /// Bulk update a collection of objects into the database.
        /// </summary>
        /// <param name="context">The EntityFramework DbContext object.</param>
        /// <param name="entities">The collection of objects to be updated.</param>
        /// <typeparam name="TEntity">The type of the objects collection. TEntity must be a class.</typeparam>
        /// <returns>The number of affected rows.</returns>
        public static int BulkInsertOrUpdate<TEntity>(this DbContext context, IEnumerable<TEntity> entities, InsertOptions options = InsertOptions.Default) where TEntity : class
        {
            return OperationFactory.BulkInsertOrUpdate.CommitTransaction(context.GetContextWrapper<TEntity>(), entities, options.ToSharedOptions());
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
            return OperationFactory.BulkDelete.CommitTransaction(context.GetContextWrapper<TEntity>(), entities);
        }
    }
}