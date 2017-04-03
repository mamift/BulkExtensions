using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityFramework.BulkExtensions.Metadata;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class ContextMetadataExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        internal static string GetTableName<TEntity>(this DbContext context) where TEntity : class
        {
            var metadata = context.Metadata<TEntity>();
            return $"{metadata.Schema}.{metadata.TableName}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<PropertyMetadata> GetTablePKs<TEntity>(this DbContext context) where TEntity : class
        {
            var metadata = context.Metadata<TEntity>();
            return metadata.Pks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        internal static IEnumerable<PropertyMetadata> GetTableColumns<TEntity>(this DbContext context) where TEntity : class
        {
            var entityMap = context.Metadata<TEntity>();
            return entityMap.Properties.ToList();
        }

        public static bool Exists<TEntity>(this IObjectContextAdapter context) where TEntity : class
        {
            var entityName = typeof(TEntity).Name;
            var workspace = context.ObjectContext.MetadataWorkspace;
            return workspace.GetItems<EntityType>(DataSpace.CSpace).Any(e => e.Name == entityName);
        }
    }
}