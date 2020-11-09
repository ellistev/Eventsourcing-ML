namespace Infrastructure.EventStore.Abstractions
{
    public interface ISubscription
    {
        void Stop();
    }
}