using CQRS.Essentials.Abstractions.DDD;
using Infrastructure.EventStore.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQRS.Essentials.Abstractions.ES
{
    public interface IEventStoreClient
    {
        Task<IEnumerable<IEventData>> Read(string streamId, long start = 0, IUserCredentials userCredentials = null);
        Task Save(IAggregate aggregate, Guid id);
    }
}
