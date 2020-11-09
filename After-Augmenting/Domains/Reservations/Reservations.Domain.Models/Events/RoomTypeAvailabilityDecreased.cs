using System;

namespace Reservations.Domain.Models.Events
{
    public class RoomTypeAvailabilityDecreased
    {
        public Guid Id { get; private set; } 
        public string RoomType { get; private set; }
        public int Amount { get; private set; }

        public RoomTypeAvailabilityDecreased(Guid id, string roomType, int amount)
        {
            Id = id;
            RoomType = roomType;
            Amount = amount;
        }
    }
}
