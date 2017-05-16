using System;

namespace EntityFramework.BulkExtensions.Commons.Exceptions
{
    public class BulkException : Exception
    {
        public BulkException(string message) : base(message)
        {
        }
    }
}