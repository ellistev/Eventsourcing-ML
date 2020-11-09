using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Essentials.Abstractions.CQRS;
using CQRS.Essentials.Abstractions.DDD;
using CQRS.Essentials.Abstractions.ES;
using Reservations.Domain.Aggregates;
using Reservations.Domain.Models.Commands;

namespace Reservations.Api.Handlers.Command
{
    public class ViewRoomCommandHandler : ICommandHandler<ViewRoom>
    {
        private readonly IAggregateFactory<Reservation> _reservationFactory;
        private readonly IEventStoreClient _eventStoreClient;

        public ViewRoomCommandHandler(IAggregateFactory<Reservation> reservationFactory, IEventStoreClient eventStoreClient)
        {
            _reservationFactory = reservationFactory;
            _eventStoreClient = eventStoreClient;
        }

        public async Task<object[]> Handle(ViewRoom command, CancellationToken cancellationToken)
        {
            var roomId = command.ReservationId;
            //use factory to get entity info
            var room = await _reservationFactory.Get(roomId, cancellationToken);
            //do something on reservation to raise events
            var events = room.View(command);
            //persist and publish
            await _eventStoreClient.Save(room, roomId);
            //return events
            return events.ToArray();
        }
    }
}