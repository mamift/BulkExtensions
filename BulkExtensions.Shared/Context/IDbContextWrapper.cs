using System.Data;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Context
{
    internal interface IDbContextWrapper
    {
        int Timeout { get; }
        int BatchSize { get; }
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }
        IEntityMapping EntityMapping { get; }

        int ExecuteSqlCommand(string command);

        IDataReader SqlQuery(string command);

        void Commit();

        void Rollback();
    }
}