using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.EventStore.Abstractions
{
    public interface IEventStore
    {
        Task<IEnumerable<IEventData>> Read(string streamId, long start = 0, IUserCredentials userCredentials = null);

        Task<ReadBatchResult> ReadBatch(string streamId, long start, int count,
            IUserCredentials userCredentials = null);

        Task<long> Save(string streamId, IEnumerable<object> events, long expectedVersion, Metadata metadata,
            IUserCredentials userCredentials = null);

        ISubscription SubscribeToAllFrom(IPosition lastCheckPoint, Func<ISubscription, IEventData, Task> onEventAppeared,
            Action<ISubscription> onLiveProcessingStarted = null,
            Action<ISubscription, string, Exception> onSubscriptionDropped = null, 
            IUserCredentials userCredentials = null, int batchSize = 100);

        Task<ISubscription> SubscribeToAll(Func<ISubscription, IEventData, Task> onEventAppeared,
            Action<ISubscription, string, Exception> onSubscriptionDropped = null,
            IUserCredentials userCredentials = null);
    }
}