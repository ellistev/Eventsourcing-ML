using System;

namespace Infrastructure.EventStore.Abstractions
{
    public class EventData : IEventData
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public string StreamId { get; set; }
        public long EventNumber { get; set; }
        public object Event { get; set; }
        public IMetadataProvider Metadata { get; set; }
        public IPosition Position { get; set; }
    }
}