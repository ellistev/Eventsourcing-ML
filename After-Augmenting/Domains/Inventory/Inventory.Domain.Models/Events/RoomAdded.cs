using System;

namespace Inventory.Domain.Models.Events
{
    public class RoomAdded
    {
        public Guid Id { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }

        public RoomAdded(Guid id, Guid hotelId, string roomType)
        {
            Id = id;
            HotelId = hotelId;
            RoomType = roomType;
        }
    }
}
