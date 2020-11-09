using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.Room{
    public class RoomTemperatureViewsReadModel
    {
        [PartitionKey]
        public Guid HotelId { get; set; }
        [RowKey]
        public Guid ViewId { get; set; }
        public Guid ReservationId { get; set; }
        public DateTime ViewDateTime { get; set; }
        public DateTime? ReservationDateTime { get; set; }
        public string RoomType { get; set; }
        public int HotelLocationTemp { get; set; }
        public int UserLocationTemp { get; set; }
        public double MillisecondsToReservation { get; set; }
    }
}