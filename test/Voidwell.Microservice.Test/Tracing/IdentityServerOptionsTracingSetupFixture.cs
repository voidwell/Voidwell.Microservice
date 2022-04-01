namespace Voidwell.Microservice.Test.Tracing
{
    public class IdentityServerOptionsTracingSetupFixture : IDisposable
    {
        public TestIdentityProvider IdentityProvider { get; }
        public DelegatingHandler WrappedMessageHandler { get; private set; }
        public Func<HttpRequestMessage, Task> WrappedMessageHandlerAction { get; set; }

        public IdentityServerOptionsTracingSetupFixture()
        {
            IdentityProvider = new TestIdentityProvider();
        }

        public void ResetFixture()
        {
            WrappedMessageHandlerAction = hrm => Task.CompletedTask;
            WrappedMessageHandler = new FakeWrappedHttpMessageHandler(hrm => WrappedMessageHandlerAction(hrm),
                IdentityProvider.CreateMessageHandler());
        }

        public void Dispose()
        {
            WrappedMessageHandler.Dispose();
            IdentityProvider.Dispose();
        }
    }
}
