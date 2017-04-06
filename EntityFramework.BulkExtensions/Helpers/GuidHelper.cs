using System;

namespace EntityFramework.BulkExtensions.Helpers
{
    internal static class GuidHelper
    {
        private const int RandomLength = 6;

        internal static string GetRandomTableGuid()
        {
            return Guid.NewGuid().ToString().Substring(0, RandomLength);
        }
    }
}