using System.Data.Entity;

namespace BulkExtensions.Tests.Database
{
    public class TestDatabaseInitializer : DropCreateDatabaseAlways<TestDatabase>
    {
        public TestDatabaseInitializer()
        {
            
        }
    }
}