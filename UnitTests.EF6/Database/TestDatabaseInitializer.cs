using System.Data.Entity;

namespace UnitTests.EF6.Database
{
    public class TestDatabaseInitializer : DropCreateDatabaseIfModelChanges<TestDatabase>
    {
        protected override void Seed(TestDatabase context)
        {
            context.Database.ExecuteSqlCommand(
                "alter table ComputedEntity add constraint df_ComputedDate default getutcdate() for [CreatedUtc]");
            base.Seed(context);
        }
    }
}