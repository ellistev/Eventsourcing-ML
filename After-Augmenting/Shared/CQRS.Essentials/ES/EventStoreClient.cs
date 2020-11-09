using CQRS.Essentials.Abstractions.CQRS;
using CQRS.Essentials.Abstractions.DDD;
using CQRS.Essentials.Abstractions.ES;
using Infrastructure.EventStore.Abstractions;
using Infrastructure.Publishers.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Essentials.ES
{
    /// <summary>
    /// Event store client that is a wrapper around the event store
    /// </summary>
    public class EventStoreClient : IEventStoreClient
    {
        private readonly IBus _bus;
        private readonly IEventStore _eventStore;
        private readonly IPublisher _externalEventPublisher;
        private readonly AsyncLocal<bool> _processing = new AsyncLocal<bool>();

        public EventStoreClient(IBus bus, IEventStore eventStore, IPublisher externalEventPublisher)
        {
            _bus = bus;
            _eventStore = eventStore;
            _externalEventPublisher = externalEventPublisher;
        }

        public async Task<IEnumerable<IEventData>> Read(string streamId, long start = 0, IUserCredentials userCredentials = null)
        {
            return await _eventStore.Read(streamId, start, userCredentials);
        }

        public async Task Save(IAggregate aggregate, Guid id)
        {
            var streamId = aggregate.GetType().Name + "-" + id;
            var events = aggregate.UnCommitedEvents.ToArray();
            var expectedVersion = aggregate.Version - aggregate.UnCommitedEvents.Count();
            var metadata = new Metadata();
            metadata.Add("$correlationId", Guid.NewGuid().ToString());

            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            metadata.Add("Timestamp", (DateTime.UtcNow.Ticks - unixEpoch.Ticks) / TimeSpan.TicksPerMillisecond);
            var nextExpectedVersion = await _eventStore.Save(streamId, events, expectedVersion, metadata);

            if (expectedVersion == nextExpectedVersion) return; //nothing to publish already was published versions match
            
            if (_processing.Value)
            {
                // If we're already executing in this async context we don't wait on the publish otherwise we end up with a deadlock
#pragma warning disable 4014
                Publish(streamId, nextExpectedVersion, expectedVersion);
#pragma warning restore 4014
            }
            else
            {
                _processing.Value = true;
                try
                {
                    await Publish(streamId, nextExpectedVersion, expectedVersion);
                }
                finally
                {
                    _processing.Value = false;
                }
            }
        }

        private async Task Publish(string streamId, long nextExpectedVersion, long expectedVersion)
        {
            var count = (int)(nextExpectedVersion - expectedVersion);
            var readRes = await _eventStore.ReadBatch(streamId, expectedVersion + 1, count);
            foreach (var eventData in readRes.Events)
            {
                //put event data on internal direct bus
                await _bus.Publish(eventData); 
                //publish events on to external bus such as service bus
                await _externalEventPublisher.Publish(eventData);
            }
        }
    }
}