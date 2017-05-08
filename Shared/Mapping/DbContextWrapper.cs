using System.Collections.Generic;
using System.Data;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
    internal class DbContextWrapper : IDbContextWrapper
    {
        internal DbContextWrapper(IDbConnection connection, IDbTransaction transaction, IEntityMapping entityMapping)
        {
            Connection = connection;
            IsInternalTransaction = transaction == null;
            Transaction = transaction ?? connection.BeginTransaction();
            EntityMapping = entityMapping;
        }

        public IEntityMapping EntityMapping { get; }
        public IDbConnection Connection { get; }
        public IDbTransaction Transaction { get; }
        private bool IsInternalTransaction { get; }

        public int ExecuteSqlCommand(string command)
        {
            var sqlCommand = Connection.CreateCommand();
            sqlCommand.Transaction = Transaction;
            sqlCommand.CommandTimeout = Connection.ConnectionTimeout;

            return sqlCommand.ExecuteNonQuery();
        }

        public IEnumerable<T> SqlQuery<T>(string command) where T : struct
        {
            var list = new List<T>();
            var sqlCommand = Connection.CreateCommand();
            sqlCommand.Transaction = Transaction;
            sqlCommand.CommandTimeout = Connection.ConnectionTimeout;

            using (var reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add((T) reader.GetValue(0));
                }
            }

            return list;
        }

        public void Commit()
        {
            if (IsInternalTransaction)
                Transaction.Commit();
        }

        public void Rollback()
        {
            if (IsInternalTransaction)
                Transaction.Rollback();
        }
    }
}