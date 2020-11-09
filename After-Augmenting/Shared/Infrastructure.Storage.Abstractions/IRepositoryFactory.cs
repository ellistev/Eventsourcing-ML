using System;

namespace Infrastructure.Storage.Abstractions
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity> Create<TEntity>() where TEntity : class;
        object Create(Type entityType);
        object CreateLookUp(Type modelType);
    }
}