using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
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
    }
}
