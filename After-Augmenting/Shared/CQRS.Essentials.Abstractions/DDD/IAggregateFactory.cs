using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Essentials.Abstractions.DDD
{
    public interface IAggregateFactory<TResult> where TResult : IAggregate, new()
    {
        Task<TResult> Get(Guid id, CancellationToken cancellationToken);
    }
}
