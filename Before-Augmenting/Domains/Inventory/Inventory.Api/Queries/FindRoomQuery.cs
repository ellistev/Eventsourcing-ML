using CQRS.Essentials.Abstractions.CQRS;
using Inventory.Domain.ReadModels.Rooms;
using System;

namespace Inventory.Api.Queries
{
    public class FindRoomQuery : IQuery<RoomsReadModel>
    {
        public Guid HotelId { get; set; }
        public Guid RoomId { get; set; }
    }
}
