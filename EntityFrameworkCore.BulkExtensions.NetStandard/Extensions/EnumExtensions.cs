using EntityFramework.BulkExtensions.Commons.Helpers;

namespace EntityFrameworkCore.BulkExtensions.Extensions
{
    internal static class EnumExtensions
    {
        internal static BulkOptions ToSharedOptions(this InsertOptions option)
        {
            return (BulkOptions)(int)option;
        }

        internal static BulkOptions ToSharedOptions(this UpdateOptions option)
        {
            return (BulkOptions)(int)option;
        }
    }
}