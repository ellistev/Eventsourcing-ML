using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Inventory.Domain.Models.Commands;
using Inventory.Domain.Aggregates;
using CQRS.Essentials.Abstractions.CQRS;
using CQRS.Essentials.Abstractions.DDD;
using CQRS.Essentials.Abstractions.ES;

namespace Inventory.Api.Handlers.Command
{
    public class AddRoomCommandHandler : ICommandHandler<AddRoom>
    {
        private readonly IAggregateFactory<Room> _roomFactory;
        private readonly IEventStoreClient _eventStoreClient;

        public AddRoomCommandHandler(IAggregateFactory<Room> roomFactory, IEventStoreClient eventStoreClient)
        {
            _roomFactory = roomFactory;
            _eventStoreClient = eventStoreClient;
        }

        public async Task<object[]> Handle(AddRoom command, CancellationToken cancellationToken)
        {
            var roomId = command.RoomId;
            //use factory to get entity info
            var room = await _roomFactory.Get(roomId, cancellationToken);
            //do something on housekeeping room to raise events
            var events = room.Add(command);
            //persist and publish
            await _eventStoreClient.Save(room, roomId);
            //return events
            return events.ToArray();
        }
    }
}