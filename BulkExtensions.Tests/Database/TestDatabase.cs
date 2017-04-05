using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using BulkExtensions.Tests.Model;

namespace BulkExtensions.Tests.Database
{
    public class TestDatabase : DbContext
    {
        public TestDatabase(string databaseName = "TestDatabase")
            : base(databaseName)
        {
        }

        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add<OneToManyCascadeDeleteConvention>();
            modelBuilder.Entity<Person>()
                .Map<Person>(m => m.Requires("Disc").HasValue("P"))
                .Map<Employee>(m => m.Requires("Disc").HasValue("E"))
                .Map<Client>(m => m.Requires("Disc").HasValue("C"));
        }
    }
}