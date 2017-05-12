namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
    internal static class OperationFactory
    {
        internal static IBulkOperation BulkInsert => new BulkInsert();

        internal static IBulkOperation BulkUpdate => new BulkUpdate();

        internal static IBulkOperation BulkInsertOrUpdate => new BulkInsertOrUpdate();

        internal static IBulkOperation BulkDelete => new BulkDelete();
    }
}