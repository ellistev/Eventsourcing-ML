using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.Reservation
{
    public class ReservationsReadModel
    {
        [PartitionKey]
        public Guid HotelId { get; set; }
        [RowKey]
        public Guid Id { get; set; }
        public string RoomType { get; set; }
    }
}