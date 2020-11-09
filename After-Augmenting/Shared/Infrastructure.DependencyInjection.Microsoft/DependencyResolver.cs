using System;
using Infrastructure.DependencyInjection.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection.Microsoft
{
    public class DependencyResolver : IDependencyResolver
    {
        private IServiceProvider _serviceProvider;
        private readonly IServiceCollection _serviceCollection;

        public DependencyResolver(IServiceCollection serviceCollection = null)
        {
            _serviceCollection = serviceCollection ?? new ServiceCollection();
            Init();
        }

        private void Init()
        {
            _serviceProvider = _serviceCollection.BuildServiceProvider();
        }

        public T Resolve<T>()
        {
            return _serviceProvider.GetService<T>();
        }
    }
}
