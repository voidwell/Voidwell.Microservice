using Microsoft.EntityFrameworkCore;

namespace Voidwell.Microservice.Test.EntityFramework
{
    public class TestDatabaseFixture : IDisposable
    {
        public TestDbContext FixtureDbContext { get; private set; }

        public void ResetFixture()
        {

            FixtureDbContext = CreateDbContext();

            FixtureDbContext.Items.AddRange(GenerateDbItems(5));

            FixtureDbContext.SaveChanges();
        }

        public void Dispose()
        {
            FixtureDbContext.Dispose();
        }

        private TestDbContext CreateDbContext()
        {
            var builder = new DbContextOptionsBuilder<TestDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            return new TestDbContext(builder.Options);
        }

        public IEnumerable<TestDbItem> GenerateDbItems(int amount)
        {
            return Enumerable.Range(1, amount).Select(a => new TestDbItem(a, $"test{a}", $"test-{a}"));
        }

        public class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<TestDbItem> Items { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);

                builder.Entity<TestDbItem>()
                    .HasKey(a => new { a.Value1, a.Value2 });
            }
        }

        public class TestDbItem
        { 
            public TestDbItem(int value1, string value2, string value3)
            {
                Value1 = value1;
                Value2 = value2;
                Value3 = value3;
            }

            public int Value1 { get; set; }
            public string Value2 { get; set; }
            public string Value3 { get; set; }
        }
    }
}
