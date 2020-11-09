using System;

namespace Infrastructure.EventStore.Abstractions
{
    public interface IEventData
    {
        Guid EventId { get; }
        string EventType { get; }
        string StreamId { get; }
        long EventNumber { get; }
        object Event { get; }
        IMetadataProvider Metadata { get; }
        IPosition Position { get; }
    }
}