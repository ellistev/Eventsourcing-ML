using Infrastructure.Storage.Abstractions.CustomAttributes;
using System;

namespace Reservations.Domain.ReadModels.Location
{
    public class LocationWeatherReadModel
    {
        [PartitionKey]
        public string Location { get; set; }
        [RowKey]
        public DateTime Date { get; set; }
        public int Temperature { get; set; }
    }
}