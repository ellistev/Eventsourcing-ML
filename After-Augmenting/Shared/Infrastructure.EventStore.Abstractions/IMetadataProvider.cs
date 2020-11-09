namespace Infrastructure.EventStore.Abstractions
{
    public interface IMetadataProvider
    {
        bool TryGet<T>(string name, out T value);
    }
}