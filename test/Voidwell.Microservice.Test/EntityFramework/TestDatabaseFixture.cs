using Microsoft.EntityFrameworkCore;

namespace Voidwell.Microservice.Test.EntityFramework
{
    public class TestDatabaseFixture : IDisposable
    {
        public TestDbContext FixtureDbContext { get; private set; }

        public void ResetFixture()
        {

            FixtureDbContext = CreateDbContext();

            FixtureDbContext.CompositeItems.AddRange(GenerateDbCompositeItems(5));
            FixtureDbContext.SingleItems.AddRange(GenerateDbSingleItems(3));

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

        public IEnumerable<CompositeKeyDbItem> GenerateDbCompositeItems(int amount)
        {
            return Enumerable.Range(1, amount).Select(a => new CompositeKeyDbItem(a, $"test{a}", $"test-{a}"));
        }

        public IEnumerable<SingleKeyDbItem> GenerateDbSingleItems(int amount)
        {
            return Enumerable.Range(1, amount).Select(a => new SingleKeyDbItem(a, $"test{a}"));
        }

        public class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<CompositeKeyDbItem> CompositeItems { get; set; }
            public DbSet<SingleKeyDbItem> SingleItems { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                base.OnModelCreating(builder);

                builder.Entity<SingleKeyDbItem>()
                    .HasKey(a => a.Value1);

                builder.Entity<CompositeKeyDbItem>()
                    .HasKey(a => new { a.Value1, a.Value2 });
            }
        }

        public class CompositeKeyDbItem
        { 
            public CompositeKeyDbItem(int value1, string value2, string value3)
            {
                Value1 = value1;
                Value2 = value2;
                Value3 = value3;
            }

            public int Value1 { get; set; }
            public string Value2 { get; set; }
            public string Value3 { get; set; }
        }

        public class SingleKeyDbItem
        {
            public SingleKeyDbItem(int value1, string value2)
            {
                Value1 = value1;
                Value2 = value2;
            }

            public int Value1 { get; set; }
            public string Value2 { get; set; }
        }
    }
}
