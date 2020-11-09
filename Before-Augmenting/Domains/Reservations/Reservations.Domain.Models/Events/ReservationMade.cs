using System;

namespace Reservations.Domain.Models.Events
{
    public class ReservationMade
    {
        public Guid Id { get; private set; }
        public Guid HotelId { get; private set; }
        public Guid ViewId { get; private set; }
        public string RoomType { get; private set; }
        public DateTime Timestamp { get; private set; }

        public ReservationMade(Guid id, Guid hotelId, Guid viewId, string roomType, DateTime timestamp)
        {
            Id = id;
            ViewId = viewId;
            HotelId = hotelId;
            RoomType = roomType;
            Timestamp = timestamp;
        }
    }
}
