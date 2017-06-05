using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using UnitTests.EF6.Model;

namespace UnitTests.EF6.Database
{
    public class TestDatabase : DbContext
    {

        public TestDatabase() : base("TestDatabaseWork")
        {
        }
        
        public DbSet<Person> People { get; set; }
        public DbSet<SimpleModel> SimpleModel { get; set; }
        public DbSet<FirstKeyEntity> FirstKeyEntities { get; set; }
        public DbSet<SecondKeyEntity> SecondKeyEntities { get; set; }
        public DbSet<CompositeKeyEntity> CompositeKeyEntities { get; set; }
        public DbSet<CustomSchemaEntity> CustomSchemaEntities { get; set; }
        public DbSet<NotIncrementIdEntity> NotIncrementIdEntities { get; set; }
        public DbSet<ComputedEntity> ComputedEntities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}