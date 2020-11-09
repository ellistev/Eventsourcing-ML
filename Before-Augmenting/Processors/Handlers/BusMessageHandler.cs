using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Threading;
using Newtonsoft.Json;
using System.Text;
using CQRS.Essentials.Abstractions.CQRS;
using Inventory.Domain.Models.Events;
using Infrastructure.EventStore.Abstractions;
using Reservations.Domain.Models.Events;

namespace HotelManagement.Processor.Handlers
{
    public class BusMessageHandler
    {
        private readonly IBus _bus;
        public BusMessageHandler(IBus bus)
        {
            _bus = bus;
        }

        private const string eventTypeMessagePropertyKey = "EventType";
        public async Task Handle(Message message, CancellationToken cancellationToken)
        {
            if (message.UserProperties.TryGetValue(eventTypeMessagePropertyKey, out var eventType))
            {
                if(eventType.ToString().Equals(typeof(RoomAdded).Name, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    var roomAddedEvent = JsonConvert.DeserializeObject<RoomAdded>(Encoding.Default.GetString(message.Body));
                    //translate the inventory event to reservations related event for use by event handlers within reservations
                    var roomTypeAvailabilityIncreased = new RoomTypeAvailabilityIncreased(roomAddedEvent.HotelId, roomAddedEvent.RoomType, 1);
                    await _bus.Publish(new EventData
                    {
                        EventType = roomTypeAvailabilityIncreased.GetType().Name,
                        Event = roomTypeAvailabilityIncreased,
                        Metadata = new Metadata()
                    });
                }
            }
        }
    }
}