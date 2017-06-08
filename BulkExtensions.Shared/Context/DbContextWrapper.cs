using System.Data;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Context
{
    internal class DbContextWrapper : IDbContextWrapper
    {
        internal DbContextWrapper(IDbConnection connection, IDbTransaction transaction, IEntityMapping entityMapping)
        {
            Connection = connection;
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            IsInternalTransaction = transaction == null;
            Transaction = transaction ?? connection.BeginTransaction();
            EntityMapping = entityMapping;
        }

        public IEntityMapping EntityMapping { get; }
        public IDbConnection Connection { get; }
        public IDbTransaction Transaction { get; }
        private bool IsInternalTransaction { get; }

        private const int MinimumTimeout = 60;
        public int Timeout => Connection.ConnectionTimeout > MinimumTimeout
            ? Connection.ConnectionTimeout
            : MinimumTimeout;

        public int BatchSize { get; set; } = 5000;

        public int ExecuteSqlCommand(string command)
        {
            var sqlCommand = CreateCommand(command);
            return sqlCommand.ExecuteNonQuery();
        }

        public IDataReader SqlQuery(string command)
        {
            var sqlCommand = CreateCommand(command);
            return sqlCommand.ExecuteReader();
        }

        private IDbCommand CreateCommand(string command)
        {
            var sqlCommand = Connection.CreateCommand();
            sqlCommand.Transaction = Transaction;
            sqlCommand.CommandTimeout = Timeout;
            sqlCommand.CommandText = command;
            return sqlCommand;
        }

        public void Commit()
        {
            if (IsInternalTransaction)
            {
                Transaction.Commit();
                Transaction.Dispose();
            }
        }

        public void Rollback()
        {
            if (IsInternalTransaction)
            {
                Transaction.Dispose();
            }
        }
    }
}