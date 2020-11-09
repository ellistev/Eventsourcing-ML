using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.Room{
    public class RoomTemperatureViewsReadModel
    {
        [PartitionKey]
        public Guid RoomId { get; set; }
        [RowKey]
        public Guid ViewId { get; set; }
        public DateTime ViewDateTime { get; set; }
        public DateTime? ReservationDateTime { get; set; }
        public string HotelLocationTemp { get; set; }
        public string UserLocationTemp { get; set; }
    }
}