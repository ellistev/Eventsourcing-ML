using Infrastructure.DependencyInjection.Abstractions;
using Infrastructure.Storage.Abstractions;
using System;

namespace Infrastructure.Storage.AzureTableStorage
{
    public class RepositoryFactory: IRepositoryFactory
    {
        private readonly IDependencyResolver _dependencyResolver;

        public RepositoryFactory(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public IRepository<TEntity> Create<TEntity>() where TEntity : class
        {
            return _dependencyResolver.Resolve<IRepository<TEntity>>();
        }

        public object Create(Type entityType)
        {
            var methodInfo = GetType().GetMethod("Create");
            var typedMethod = methodInfo.MakeGenericMethod(entityType);
            return typedMethod.Invoke(this, new object[] { });
        }
    }
}