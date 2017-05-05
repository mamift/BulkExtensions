using System.Data.Entity;

namespace EntityFramework.BulkExtensions.UnitTests.Database
{
    public class TestDatabaseInitializer : DropCreateDatabaseAlways<TestDatabase>
    {
    }
}