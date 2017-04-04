using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace EntityFramework.BulkExtensions.Extensions
{
    /// <summary>
    /// </summary>
    internal static class ContextExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static DbContextTransaction InternalTransaction(this DbContext context)
        {
            DbContextTransaction transaction = null;
            if (context.Database.CurrentTransaction == null)
            {
                transaction = context.Database.BeginTransaction();
            }
            return transaction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        internal static void UpdateEntityState<TEntity>(this DbContext context, IEnumerable<TEntity> collection) where TEntity : class
        {
            try
            {
                var list = collection.ToList();
                context.Configuration.AutoDetectChangesEnabled = false;

                foreach (var entity in list)
                {
                    context.Entry(entity).State = EntityState.Unchanged;
                }
            }
            finally
            {
                context.Configuration.AutoDetectChangesEnabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        internal static void DetachEntityFromContext<TEntity>(this DbContext context, IEnumerable<TEntity> collection) where TEntity : class
        {
            try
            {
                var list = collection.ToList();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (var entity in list)
                {
                    objectContext.Detach(entity);
                }
            }
            finally
            {
                context.Configuration.AutoDetectChangesEnabled = true;
            }
        }
    }
}