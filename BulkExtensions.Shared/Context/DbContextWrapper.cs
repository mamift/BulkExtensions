using System.Data;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Context
{
    internal class DbContextWrapper : IDbContextWrapper
    {
        private const int DefaultTimeout = 60;
        private const int DefaultBatchSize = 5000;

        internal DbContextWrapper(IDbConnection connection, IDbTransaction transaction, IEntityMapping entityMapping, int? commandTimeout)
        {
            Connection = connection;
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Timeout = commandTimeout ?? DefaultTimeout;
            IsInternalTransaction = transaction == null;
            Transaction = transaction ?? connection.BeginTransaction();
            EntityMapping = entityMapping;
        }

        public IEntityMapping EntityMapping { get; }
        public IDbConnection Connection { get; }
        public IDbTransaction Transaction { get; }
        private bool IsInternalTransaction { get; }

        private int _currentTimeout;
        public int Timeout
        {
            get => _currentTimeout;
            private set => _currentTimeout = value > DefaultTimeout ? value : DefaultTimeout;
        }

        public int BatchSize { get; } = DefaultBatchSize;

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