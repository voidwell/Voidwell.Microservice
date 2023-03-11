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
        public async Task UpsertRangeWithoutNullPropertiesAsync_CompositeKey_ObjectArray()
        {
            // Arrange
            var testEntities = new List<TestDatabaseFixture.CompositeKeyDbItem>
            {
                new TestDatabaseFixture.CompositeKeyDbItem(1, "test1", null),
                new TestDatabaseFixture.CompositeKeyDbItem(3, "test3", "test-three"),
                new TestDatabaseFixture.CompositeKeyDbItem(6, "test6", "test-6")
            };

            var expectedEntities = new[]
            {
                new TestDatabaseFixture.CompositeKeyDbItem(1, "test1", "test-1"),
                new TestDatabaseFixture.CompositeKeyDbItem(2, "test2", "test-2"),
                new TestDatabaseFixture.CompositeKeyDbItem(3, "test3", "test-three"),
                new TestDatabaseFixture.CompositeKeyDbItem(4, "test4", "test-4"),
                new TestDatabaseFixture.CompositeKeyDbItem(5, "test5", "test-5"),
                new TestDatabaseFixture.CompositeKeyDbItem(6, "test6", "test-6")
            };

            // Act
            await _fixture.FixtureDbContext.UpsertRangeWithoutNullPropertiesAsync(testEntities);

            // Assert
            var dbItems = await _fixture.FixtureDbContext.CompositeItems.ToListAsync();

            dbItems.Should()
                .BeEquivalentTo(expectedEntities);
        }

        [Theory]
        [InlineData(null, "test-1")]
        [InlineData("test-one", "test-one")]
        [InlineData("test-1", "test-1")]
        public async Task UpsertRangeWithoutNullPropertiesAsync_CompositeKey_Object(string testValue, string expectedValue)
        {
            // Arrange
            var testEntity = new TestDatabaseFixture.CompositeKeyDbItem(1, "test1", testValue);

            var expectedEntities = new[]
            {
                new TestDatabaseFixture.CompositeKeyDbItem(1, "test1", expectedValue),
                new TestDatabaseFixture.CompositeKeyDbItem(2, "test2", "test-2"),
                new TestDatabaseFixture.CompositeKeyDbItem(3, "test3", "test-3"),
                new TestDatabaseFixture.CompositeKeyDbItem(4, "test4", "test-4"),
                new TestDatabaseFixture.CompositeKeyDbItem(5, "test5", "test-5")
            };

            // Act
            await _fixture.FixtureDbContext.UpsertWithoutNullPropertiesAsync(testEntity);

            // Assert
            var dbItems = await _fixture.FixtureDbContext.CompositeItems.ToListAsync();

            dbItems.Should()
                .BeEquivalentTo(expectedEntities);
        }

        [Fact]
        public async Task UpsertRangeWithoutNullPropertiesAsync_SingleKey_ObjectArray()
        {
            // Arrange
            var testEntities = new List<TestDatabaseFixture.SingleKeyDbItem>
            {
                new TestDatabaseFixture.SingleKeyDbItem(1, null),
                new TestDatabaseFixture.SingleKeyDbItem(2, "test2"),
                new TestDatabaseFixture.SingleKeyDbItem(3, "testthree"),
            };

            var expectedEntities = new[]
            {
                new TestDatabaseFixture.SingleKeyDbItem(1, "test1"),
                new TestDatabaseFixture.SingleKeyDbItem(2, "test2"),
                new TestDatabaseFixture.SingleKeyDbItem(3, "testthree")
            };

            // Act
            await _fixture.FixtureDbContext.UpsertRangeWithoutNullPropertiesAsync(testEntities);

            // Assert
            var dbItems = await _fixture.FixtureDbContext.SingleItems.ToListAsync();

            dbItems.Should()
                .BeEquivalentTo(expectedEntities);
        }

        [Theory]
        [InlineData(null, "test1")]
        [InlineData("testone", "testone")]
        [InlineData("test1", "test1")]
        public async Task UpsertRangeWithoutNullPropertiesAsync_SingleKey_Object(string testValue, string expectedValue)
        {
            // Arrange
            var testEntity = new TestDatabaseFixture.SingleKeyDbItem(1, testValue);

            var expectedEntities = new[]
            {
                new TestDatabaseFixture.SingleKeyDbItem(1, expectedValue),
                new TestDatabaseFixture.SingleKeyDbItem(2, "test2"),
                new TestDatabaseFixture.SingleKeyDbItem(3, "test3")
            };

            // Act
            await _fixture.FixtureDbContext.UpsertWithoutNullPropertiesAsync(testEntity);

            // Assert
            var dbItems = await _fixture.FixtureDbContext.SingleItems.ToListAsync();

            dbItems.Should()
                .BeEquivalentTo(expectedEntities);
        }
    }
}
