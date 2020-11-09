namespace Infrastructure.EventStore.Abstractions
{
    public interface IPosition
    {
        int CompareTo(IPosition other);
        string ToString();
    }
}