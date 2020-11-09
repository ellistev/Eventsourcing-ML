using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.Hotel
{
    public class HotelsReadModel
    {
        [PartitionKey]
        public Guid HotelId { get; set; }
        [RowKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
    }
}