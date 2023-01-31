using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace Voidwell.Microservice.Test
{
    public static class MockExtensions
    {
        public static Mock<T> AsMock<T>(this T obj)
            where T : class
        {
            return Mock.Get(obj);
        }

        public static IReturnsResult<TMock> ReturnsNoActionTask<TMock>(this ISetup<TMock, Task> setup)
            where TMock : class
        {
            return setup.Returns(Task.FromResult(0));
        }

        public static IReturnsResult<TMock> ReturnsNullAsync<TMock, TResult>(this IReturns<TMock, Task<TResult?>> mock)
            where TMock : class
            where TResult : class
        {
            return mock.ReturnsAsync(null as TResult);
        }
    }
}
