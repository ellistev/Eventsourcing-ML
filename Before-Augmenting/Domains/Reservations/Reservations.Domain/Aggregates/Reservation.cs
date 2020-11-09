using Reservations.Domain.Models.Events;
using Reservations.Domain.Models.Commands;
using System;
using System.Collections.Generic;
using CQRS.Essentials.DDD;

namespace Reservations.Domain.Aggregates
{
    public class Reservation : AggregateBase<Reservation.State>
    {
        public Reservation()
        {
            RegisterTransition<ReservationMade>(Apply);
            RegisterTransition<RoomViewed>(Apply);
        }

        public struct State
        {
            public Guid Id { get; set; }
            public Guid HotelId { get; set; }
            public string RoomType { get; set; }
            public bool IsReserved { get; set; }
        }

        public IEnumerable<object> Make(MakeReservation command)
        {
            RaiseEvent(new ReservationMade(command.ReservationId, command.HotelId, command.ViewId, command.RoomType, DateTime.UtcNow));
            return base.UnCommitedEvents;
        }

        private State Apply(State state, ReservationMade @event)
        {
            state.Id = @event.Id;
            state.HotelId = @event.HotelId;
            state.RoomType = @event.RoomType;
            state.IsReserved = true;
            return state;
        }
        
        public IEnumerable<object> View(ViewRoom command)
        {
            RaiseEvent(new RoomViewed(command.ReservationId, command.ViewId, command.UserId, command.HotelId, command.RoomType, DateTime.UtcNow));
            return base.UnCommitedEvents;
        }

        private State Apply(State state, RoomViewed @event)
        {
            return state;
        }
    }
}
