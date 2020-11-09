using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.Reservation
{
    public class RoomTypeAvailabilityReadModel
    {
        [PartitionKey]
        public Guid HotelId { get; set; }
        [RowKey]
        public string RoomType { get; set; }
        public int Amount { get; set; }
    }
}