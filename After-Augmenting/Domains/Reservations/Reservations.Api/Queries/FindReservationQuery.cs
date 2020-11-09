using CQRS.Essentials.Abstractions.CQRS;
using Reservations.Domain.ReadModels.Reservation;
using System;

namespace Reservations.Api.Queries
{
    public class FindReservationQuery : IQuery<ReservationsReadModel>
    {
        public Guid HotelId { get; set; }
        public Guid ReservationId { get; set; }
    }
}
