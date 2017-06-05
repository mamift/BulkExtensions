using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using UnitTests.EF6.Model;

namespace UnitTests.EF6.Database
{
    public class TestDatabase : DbContext
    {

        public TestDatabase() : base("TestDatabaseLocal")
        {
        }
        
        public DbSet<Person> People { get; set; }
        public DbSet<SimpleModel> SimpleModel { get; set; }
        public DbSet<FirstKeyEntity> FirstKeyEntity { get; set; }
        public DbSet<SecondKeyEntity> SecondKeyEntity { get; set; }
        public DbSet<CompositeKeyEntity> CompositeKeyEntity { get; set; }
        public DbSet<CustomSchemaEntity> CustomSchemaEntities { get; set; }
        public DbSet<NotIncrementIdEntity> NotIncrementIdEntity { get; set; }
        public DbSet<ComputedEntity> ComputedEntity { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}