using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.Room{
    public class RoomsReadModel
    {
        [PartitionKey]
        public Guid RoomId { get; set; }
        [RowKey]
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public string Location { get; set; }
        public string RoomType { get; set; }
    }
}