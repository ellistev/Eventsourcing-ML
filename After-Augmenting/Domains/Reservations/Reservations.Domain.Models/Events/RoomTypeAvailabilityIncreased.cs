using System;

namespace Reservations.Domain.Models.Events
{
    public class RoomTypeAvailabilityIncreased
    {
        public Guid Id { get; private set; }
        public string RoomType { get; private set; }
        public int Amount { get; private set; }

        public RoomTypeAvailabilityIncreased(Guid id, string roomType, int amount)
        {
            Id = id;
            RoomType = roomType;
            Amount = amount;
        }
    }
}
