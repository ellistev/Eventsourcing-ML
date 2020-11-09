using CQRS.Essentials.Abstractions.DDD;
using CQRS.Essentials.Abstractions.ES;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Essentials.DDD
{
    public class AggregateFactory<TAggregate> : IAggregateFactory<TAggregate> where TAggregate : class, IAggregate, new()
    {
        private readonly IEventStoreClient _eventStoreClient;

        public AggregateFactory(IEventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        public async Task<TAggregate> Get(Guid id, CancellationToken cancellationToken)
        {
            var aggregate = new TAggregate();
            var streamId = aggregate.GetType().Name + "-" + id;
            var eventDatas = await _eventStoreClient.Read(streamId);

            foreach (var eventData in eventDatas)
                aggregate.Hydrate(eventData.Event); //hydrate aggregate

            return aggregate;
        }
    }
}
