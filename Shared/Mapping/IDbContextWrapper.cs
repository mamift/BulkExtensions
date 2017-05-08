using System.Collections.Generic;
using System.Data;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    internal interface IDbContextWrapper
    {
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        IEntityMapping EntityMapping { get; }

        int ExecuteSqlCommand(string command);

        IEnumerable<T> SqlQuery<T>(string command) where T : struct;

        void Commit();

        void Rollback();
    }
}