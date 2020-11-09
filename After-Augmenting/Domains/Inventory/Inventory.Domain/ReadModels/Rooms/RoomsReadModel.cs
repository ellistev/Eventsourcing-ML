using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Inventory.Domain.ReadModels.Rooms
{
    public class RoomsReadModel
    {
        [PartitionKey]
        public Guid HotelId { get; set; }
        [RowKey]
        public Guid Id { get; set; }
        public string RoomType { get; set; }

    }
}