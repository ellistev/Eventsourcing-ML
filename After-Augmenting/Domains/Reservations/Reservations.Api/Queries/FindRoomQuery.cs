using System;
using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.ReadModels.Room;

namespace Reservations.Api.Queries
{
    public class FindRoomQuery : IQuery<RoomsReadModel>
    {
        public Guid HotelId { get; set; }
        public Guid RoomId { get; set; }
    }
}