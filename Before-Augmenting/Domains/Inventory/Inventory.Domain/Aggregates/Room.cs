using Inventory.Domain.Models.Events;
using Inventory.Domain.Models.Commands;
using System;
using System.Collections.Generic;
using CQRS.Essentials.DDD;

namespace Inventory.Domain.Aggregates
{
    public class Room : AggregateBase<Room.State>
    {
        public Room()
        {
            RegisterTransition<RoomAdded>(Apply);
        }

        public struct State
        {
            public Guid Id { get; set; }
            public Guid HotelId { get; set; }
            public string RoomType { get; set; }
        }

        public IEnumerable<object> Add(AddRoom command)
        {
            RaiseEvent(new RoomAdded(command.RoomId, command.HotelId, command.RoomType));
            return UnCommitedEvents;
        }

        private State Apply(State state, RoomAdded @event)
        {
            state.Id = @event.Id;
            state.HotelId = @event.HotelId;
            state.RoomType = @event.RoomType;
            return state;
        }
    }
}
