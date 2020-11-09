namespace Infrastructure.EventStore.Abstractions
{
    public class ReadBatchResult
    {
        public readonly string StreamId;
        public readonly long FromEventNumber;
        public readonly long NextEventNumber;
        public readonly bool IsEndOfStream;
        public readonly IEventData[] Events;

        public ReadBatchResult(string streamId, long fromEventNumber, long nextEventNumber, bool isEndOfStream, IEventData[] events)
        {
            StreamId = streamId;
            FromEventNumber = fromEventNumber;
            NextEventNumber = nextEventNumber;
            IsEndOfStream = isEndOfStream;
            Events = events;
        }
    }
}