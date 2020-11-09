using System;

namespace Reservations.Domain.Models.Events
{
    public class RoomViewed
    {
        public Guid ReservationId { get; private set; }
        public Guid ViewId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }
        public DateTime TimeStamp { get; private set; }

        public RoomViewed(Guid reservationId, Guid viewId, Guid userId, Guid hotelId, string roomType, DateTime timeStamp)
        {
            ReservationId = reservationId;
            ViewId = viewId;
            UserId = userId;
            HotelId = hotelId;
            RoomType = roomType;
            TimeStamp = timeStamp;
        }
    }
}