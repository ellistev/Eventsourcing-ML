using EventStore.ClientAPI;
using Infrastructure.EventStore.Abstractions;

namespace Infrastructure.EventStore
{
    public class CatchupSubscription : ISubscription 
    {
        private readonly EventStoreCatchUpSubscription _value;

        public CatchupSubscription(EventStoreCatchUpSubscription esSubscription)
        {
            _value = esSubscription;
        }

        public void Stop()
        {
            _value.Stop();
        }
    }
}