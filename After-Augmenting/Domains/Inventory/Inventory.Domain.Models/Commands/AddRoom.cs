using CQRS.Essentials.Abstractions.CQRS;
using System;

namespace Inventory.Domain.Models.Commands
{
    public class AddRoom : ICommand
    {
        public Guid RoomId { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }

        public AddRoom(Guid id, Guid hotelId, string roomType)
        {
            RoomId = id;
            HotelId = hotelId;
            RoomType = roomType;
        }
    }
}
