using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Infrastructure.EventStore.Abstractions;
using Newtonsoft.Json;
using EventData = EventStore.ClientAPI.EventData;

namespace Infrastructure.EventStore
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreConnection _connection;
        private readonly JsonSerializer _jsonSerializer;
        private readonly int _batchSize;
        private readonly IDictionary<string, ISubscription> _subscriptions;

        private static int MAX_LIVE_QUEUE_SIZE = 10000;
        private static int READ_BATCH_SIZE = 512;

        public EventStore(IEventStoreConnection connection)
        {
            _connection = connection;
            _jsonSerializer = JsonSerializer.CreateDefault();
            _subscriptions = new Dictionary<string, ISubscription>();
            _batchSize = READ_BATCH_SIZE;
        }

        public async Task<IEnumerable<IEventData>> Read(string streamId, long start = 0, IUserCredentials userCredentials = null)
        {
            var userCreds = userCredentials == null
                ? null
                : new global::EventStore.ClientAPI.SystemData.UserCredentials(userCredentials.Username, userCredentials.Password);
            var result = await _connection.ReadStreamEventsForwardAsync(streamId, start, _batchSize, true, userCreds);
            return result.Events.Select(FromDb);
        }

        public async Task<ReadBatchResult> ReadBatch(string streamId, long start, int count, IUserCredentials userCredentials = null)
        {
            var userCreds = userCredentials == null
                ? null
                : new global::EventStore.ClientAPI.SystemData.UserCredentials(userCredentials.Username, userCredentials.Password);
            var result = await _connection.ReadStreamEventsForwardAsync(streamId, start, count, true, userCreds);
            var events = result.Events.Select(FromDb).ToArray();
            return new ReadBatchResult(result.Stream, result.FromEventNumber, result.NextEventNumber, result.IsEndOfStream, events);
        }

        private IEventData FromDb(ResolvedEvent resolvedEvent)
        {
            var isSystemEvent = resolvedEvent.Event.EventType[0] == '$';
            var metadata = new Metadata(Deserialize(resolvedEvent.Event.Metadata) ?? new Dictionary<string, object>());
            if (isSystemEvent)
            {
                return new Infrastructure.EventStore.Abstractions.EventData
                {
                    EventId = resolvedEvent.Event.EventId,
                    EventType = resolvedEvent.Event.EventType,
                    StreamId = resolvedEvent.Event.EventStreamId,
                    EventNumber = resolvedEvent.Event.EventNumber,
                    Event = Deserialize(resolvedEvent.Event.Data),
                    Metadata = metadata,
                    Position = resolvedEvent.OriginalPosition.HasValue 
                        ? new Position(resolvedEvent.OriginalPosition.Value) 
                        : new Position(global::EventStore.ClientAPI.Position.End)
                };
            }
            
            if (!metadata.TryGet("SerializedType", out string serializedTypeRaw))
            {
                throw new InvalidOperationException(
                    $"SerializedType is missing for event {resolvedEvent.Event.EventNumber}@{resolvedEvent.Event.EventStreamId}");
            }

            var serializedType = Type.GetType(serializedTypeRaw);

            return new Infrastructure.EventStore.Abstractions.EventData
            {
                EventId = resolvedEvent.Event.EventId,
                EventType = resolvedEvent.Event.EventType,
                StreamId = resolvedEvent.Event.EventStreamId,
                EventNumber = resolvedEvent.Event.EventNumber,
                Event = Deserialize(resolvedEvent.Event.Data, serializedType),
                Metadata = metadata,
                Position = resolvedEvent.OriginalPosition.HasValue 
                    ? new Position(resolvedEvent.OriginalPosition.Value) 
                    : new Position(global::EventStore.ClientAPI.Position.End)
            };
        }

        public async Task<long> Save(string streamId, IEnumerable<object> events, long expectedVersion, Metadata metadata,
            IUserCredentials userCredentials = null)
        {
            var userCreds = userCredentials == null
                ? null
                : new global::EventStore.ClientAPI.SystemData.UserCredentials(userCredentials.Username, userCredentials.Password);
            var eventDatas = events.Select(x => ToDb(x, metadata)).ToArray();
            var result = await _connection.AppendToStreamAsync(streamId, expectedVersion, eventDatas, userCreds);
            return result.NextExpectedVersion;
        }

        public ISubscription SubscribeToAllFrom(IPosition lastCheckPoint,
            Func<ISubscription, IEventData, Task> onEventAppeared, Action<ISubscription> onLiveProcessingStarted = null,
            Action<ISubscription, string, Exception> onSubscriptionDropped = null, IUserCredentials userCredentials = null,
            int batchSize = 512)
        {
            var effectiveCheckPoint =
                lastCheckPoint == null ? global::EventStore.ClientAPI.Position.Start : ((Position)lastCheckPoint).Value;
            var userCreds = userCredentials == null
                ? null
                : new global::EventStore.ClientAPI.SystemData.UserCredentials(userCredentials.Username, userCredentials.Password);
            onLiveProcessingStarted = onLiveProcessingStarted ?? ((s) => { });
            onSubscriptionDropped = onSubscriptionDropped ?? ((s, r, e) => { });

            var settings = new CatchUpSubscriptionSettings(MAX_LIVE_QUEUE_SIZE, READ_BATCH_SIZE, false, true);
            var subscriptionId = Guid.NewGuid().ToString();
            var esSubscription = _connection.SubscribeToAllFrom(effectiveCheckPoint, settings,
                async (_, ev) => await onEventAppeared(_subscriptions[subscriptionId], FromDb(ev)),
                (_) => onLiveProcessingStarted(_subscriptions[subscriptionId]),
                (_, reason, ex) => onSubscriptionDropped(_subscriptions[subscriptionId], reason.ToString(), ex),
                userCreds);
            var subscription = _subscriptions[subscriptionId] = new CatchupSubscription(esSubscription);
            return subscription;
        }

        public async Task<ISubscription> SubscribeToAll(Func<ISubscription, IEventData, Task> onEventAppeared, 
            Action<ISubscription, string, Exception> onSubscriptionDropped = null,
            IUserCredentials userCredentials = null)
        {
            var subscriptionId = Guid.NewGuid().ToString();
            var creds = userCredentials != null
                ? new global::EventStore.ClientAPI.SystemData.UserCredentials(userCredentials.Username,
                    userCredentials.Password)
                : null;
            var esSubscription = await _connection.SubscribeToAllAsync(true,
                async (_, ev) => await HandleEvent(onEventAppeared, _subscriptions[subscriptionId], ev),
                (_, reason, ex) => onSubscriptionDropped(_subscriptions[subscriptionId], reason.ToString(), ex),
                creds);
            var subscription = _subscriptions[subscriptionId] = new Subscription(esSubscription);
            return subscription;
        }

        private async Task HandleEvent(Func<ISubscription, IEventData, Task> handler, ISubscription s,
            ResolvedEvent ev)
        {
            if (ev.Event.EventStreamId[0] == '$') return;
            var eventData = FromDb(ev);
            await handler(s, eventData);
        }

        private EventData ToDb(IEventData eventData)
        {
            var metadata = (Metadata)eventData.Metadata;
            var type = eventData.Event.GetType();
            metadata.Add("SerializedType", $"{type.FullName}, {type.Assembly.GetName().Name}");
            var serializedMetadata = Serialize(metadata.Value);
            var serializedEvent = Serialize(eventData.Event);
            return new EventData(eventData.EventId, eventData.EventType, true, serializedEvent, serializedMetadata);
        }

        private EventData ToDb(object @event, Metadata metadata)
        {
            var type = @event.GetType();
            var serializedEvent = Serialize(@event);
            var eventMetadata = new Metadata(metadata.Value);
            eventMetadata.Add("SerializedType", $"{type.FullName}, {type.Assembly.GetName().Name}");
            var serializedMetadata = Serialize(eventMetadata.Value);
            return new EventData(Guid.NewGuid(), type.Name, true, serializedEvent, serializedMetadata);
        }

        private byte[] Serialize(object value)
        {
            using (var ms = new MemoryStream())
            {
                using (var tw = new StreamWriter(ms))
                {
                    _jsonSerializer.Serialize(tw, value);
                }

                return ms.ToArray();
            }
        }

        private IDictionary<string, object> Deserialize(byte[] data)
        {
            try
            {
                return (IDictionary<string, object>) Deserialize(data, typeof(Dictionary<string, object>));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private object Deserialize(byte[] data, Type type)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var tr = new StreamReader(ms))
                {
                    return _jsonSerializer.Deserialize(tr, type);
                }
            }
        }
    }
}