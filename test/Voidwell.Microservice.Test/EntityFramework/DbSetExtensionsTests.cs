using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voidwell.Microservice.EntityFramework;
using Xunit;

namespace Voidwell.Microservice.Test.EntityFramework
{
    public class DbSetExtensionsTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;

        public DbSetExtensionsTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            _fixture.ResetFixture();
        }

        [Fact]
        public async Task UpsertRangeWithoutNullPropertiesAsync()
        {
            // Arrange
            var testEntities = new List<TestDatabaseFixture.TestDbItem>
            {
                new TestDatabaseFixture.TestDbItem(1, "test1", null),
                new TestDatabaseFixture.TestDbItem(3, "test3", "test-three"),
                new TestDatabaseFixture.TestDbItem(6, "test6", "test-6")
            };

            var expectedEntities = new[]
            {
                new TestDatabaseFixture.TestDbItem(1, "test1", "test-1"),
                new TestDatabaseFixture.TestDbItem(2, "test2", "test-2"),
                new TestDatabaseFixture.TestDbItem(3, "test3", "test-three"),
                new TestDatabaseFixture.TestDbItem(4, "test4", "test-4"),
                new TestDatabaseFixture.TestDbItem(5, "test5", "test-5"),
                new TestDatabaseFixture.TestDbItem(6, "test6", "test-6")
            };

            // Act

            await _fixture.FixtureDbContext.UpsertRangeWithoutNullPropertiesAsync(testEntities);

            // Assert
            var dbItems = await _fixture.FixtureDbContext.Items.ToListAsync();

            dbItems.Should()
                .BeEquivalentTo(expectedEntities);
        }
    }
}
