using Microsoft.Extensions.Options;
using System;

namespace Voidwell.Microservice.Extensions
{
    internal class PostConfigurationWrapper<TOptions> : IPostConfigureOptions<TOptions> where TOptions : class
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Action<IServiceProvider, string, TOptions> _configureAction;

        public PostConfigurationWrapper(IServiceProvider serviceProvider, Action<IServiceProvider, string, TOptions> configureAction)
        {
            _serviceProvider = serviceProvider;
            _configureAction = configureAction;
        }

        public void PostConfigure(string name, TOptions options)
        {
            _configureAction(_serviceProvider, name, options);
        }
    }
}
