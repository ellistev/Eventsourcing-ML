using EventStore.ClientAPI;
using Infrastructure.EventStore.Abstractions;

namespace Infrastructure.EventStore
{
    public class Subscription : ISubscription
    {
        private readonly EventStoreSubscription _value;

        public Subscription(EventStoreSubscription esSubscription)
        {
            _value = esSubscription;
        }

        public void Stop()
        {
            _value.Unsubscribe();
        }
    }
}