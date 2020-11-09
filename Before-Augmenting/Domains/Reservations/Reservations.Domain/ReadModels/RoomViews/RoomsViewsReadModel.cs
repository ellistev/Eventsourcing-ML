using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.RoomViews{
    public class RoomViewsReadModel
    {
        [PartitionKey]
        public Guid HotelId { get; set; }
        [RowKey]
        public Guid ViewId { get; set; }
        public Guid UserId { get; set; }
        public string RoomType { get; set; }
        public DateTime ViewDateTime { get; set; }
        public DateTime? ReserveDateTime { get; set; }
    }
}