using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Storage.Abstractions
{
    public interface IRepository<TEntity>
    {
        Task<TEntity> GetById(Guid id, CancellationToken cancellationToken);
        Task<TEntity> GetByIds(Guid partitionId, Guid rowId, CancellationToken cancellationToken);
        Task<TEntity> GetByKeys(string partitionKey, string rowKey, CancellationToken cancellationToken);
        Task Save(TEntity entity, CancellationToken cancellationToken);
    }
}