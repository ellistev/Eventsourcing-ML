using CQRS.Essentials.Abstractions.CQRS;
using System;

namespace Reservations.Domain.Models.Commands
{
    public class MakeReservation : ICommand
    {
        public Guid ReservationId { get; private set; }
        public Guid ViewId { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }

        public MakeReservation(Guid reservationId, Guid viewId, Guid hotelId, string roomType)
        {
            ReservationId = reservationId;
            ViewId = viewId;
            HotelId = hotelId;
            RoomType = roomType;
        }
    }
}
