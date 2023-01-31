using FluentAssertions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Voidwell.Microservice.Test.Cache
{
    public class CacheTests : IClassFixture<CacheFixture>
    {
        private readonly CacheFixture _fixture;

        public CacheTests(CacheFixture fixture)
        {
            _fixture = fixture;
            _fixture.ResetFixture();
        }

        [Fact]
        public async Task Cache_GetAsync_ReturnsValue()
        {
            // Arrange
            var cacheKey = "test";
            var expected = "Cache Data";

            _fixture.MockCacheGet(cacheKey, expected)
                .Verifiable();

            // Act
            var sut = _fixture.CreateSut();

            var result = await sut.GetAsync<string>(cacheKey);

            // Assert
            _fixture.RedisDatabase.AsMock().Verify();

            result.Should()
                .Be(expected);
        }

        [Fact]
        public async Task Cache_TryGetAsync_Success_ReturnsValue()
        {
            // Arrange
            var cacheKey = "test";
            var expected = "Cache Data";

            _fixture.MockCacheGet(cacheKey, expected)
                .Verifiable();

            // Act
            var sut = _fixture.CreateSut();

            object? result = null;
            var success = await sut.TryGetAsync<string>(cacheKey, value =>
            {
                result = value; 
            });

            // Assert
            _fixture.RedisDatabase.AsMock().Verify();

            success.Should()
                .BeTrue();
            result.Should()
                .Be(expected);
        }

        [Fact]
        public async Task Cache_TryGetAsync_Fails_ReturnsUnsuccessful()
        {
            // Arrange
            var cacheKey = "test";

            _fixture.RedisDatabase.AsMock()
                .Setup(a => a.StringGetAsync(cacheKey, It.IsAny<CommandFlags>()))
                .ThrowsAsync(new Exception())
                .Verifiable();

            // Act
            var sut = _fixture.CreateSut();

            bool actionCalled = false;
            var success = await sut.TryGetAsync<string>(cacheKey, value =>
            {
                actionCalled = true;
            });

            // Assert
            _fixture.RedisDatabase.AsMock().Verify();

            success.Should()
                .BeFalse();
            actionCalled.Should()
                .BeFalse();
        }

        [Fact]
        public async Task Cache_GetAsync_NullDatabase_DoesNotThrow()
        {
            // Arrange
            _fixture.CacheConnector.AsMock()
                .Setup(a => a.ConnectAsync(It.IsAny<CancellationToken>()))
                .ReturnsNullAsync();

            // Act
            var sut = _fixture.CreateSut();

            var exception = await Record.ExceptionAsync(() => sut.GetAsync<object>("test"));

            // Assert
            exception.Should()
                .BeNull();
        }
    }
}
