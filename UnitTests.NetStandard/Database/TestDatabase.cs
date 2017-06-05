using Microsoft.EntityFrameworkCore;
using UnitTests.NetStandard.Model;

namespace UnitTests.NetStandard.Database
{
    public class TestDatabase : DbContext
    {
        public DbSet<SimpleModel> SimpleModel { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<FirstKeyEntity> FirstKeyEntity { get; set; }
        public DbSet<SecondKeyEntity> SecondKeyEntity { get; set; }
        public DbSet<CompositeKeyEntity> CompositeKeyEntity { get; set; }
        public DbSet<CustomSchemaEntity> CustomSchemaEntities { get; set; }
        public DbSet<NotIncrementIdEntity> NotIncrementIdEntity { get; set; }
        public DbSet<ComputedEntity> ComputedEntity { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
//            optionsBuilder.UseSqlServer(
//                @"Data Source=tcp:192.168.0.142,1433;Initial Catalog=UnitTests;User ID=sa;Password=sa;");
            optionsBuilder.UseSqlServer(
                @"Data Source=tcp:10.0.0.194,1433;Initial Catalog=UnitTests;User ID=sa;Password=sa;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompositeKeyEntity>()
                .HasKey(c => new {c.FirstKeyEntityId, c.SecondKeyEntityId});
            modelBuilder.Entity<Person>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<Person>("Person")
                .HasValue<Employee>("Employee")
                .HasValue<Client>("Client");
        }
    }
}