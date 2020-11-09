using System;
using CQRS.Essentials.Abstractions.CQRS;

namespace Reservations.Domain.Models.Commands
{
    public class ViewRoom : ICommand
    {
        public Guid ViewId { get; }
        public Guid ReservationId { get; }
        public Guid UserId { get; private set; }
        public Guid HotelId { get; private set; }
        public string RoomType { get; private set; }

        public ViewRoom(Guid reservationId, Guid viewId, Guid userId, Guid hotelId, string roomType)
        {
            ReservationId = reservationId;
            ViewId = viewId;
            UserId = userId;
            HotelId = hotelId;
            RoomType = roomType;
        }

    }
}