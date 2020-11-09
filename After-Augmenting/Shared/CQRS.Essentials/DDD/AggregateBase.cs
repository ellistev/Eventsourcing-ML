using CQRS.Essentials.Abstractions.DDD;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CQRS.Essentials.DDD
{
    public abstract class AggregateBase<TState> : IAggregate where TState : struct
    {
        public long Version { get; private set; } = -1;
        public IEnumerable<object> UnCommitedEvents => _unCommitedEvents;
        private readonly Dictionary<Type, Func<TState, object, TState>> _transitionRoutes = new Dictionary<Type, Func<TState, object, TState>>();
        private readonly ConcurrentQueue<object> _unCommitedEvents = new ConcurrentQueue<object>();
        protected TState _state;

        public AggregateBase()
        {
            _state = new TState();
        }

        protected void RegisterTransition<T>(Func<TState, T, TState> transition) where T : class
        {
            _transitionRoutes.Add(typeof(T), (o,b) => transition(o, b as T));
        }

        protected void RaiseEvent(object @event)
        {
            ApplyEvent(_state, @event);
            _unCommitedEvents.Enqueue(@event);
        }

        protected void RaiseEvents(IEnumerable<object> @events)
        {
            foreach (object domainEvent in @events)
                RaiseEvent(domainEvent);
        }

        public void ClearUnCommitedEvents()
        {
            while (_unCommitedEvents.TryDequeue(out _)) ;
        }

        public void ApplyEvent(TState state, object @event)
        {
            var eventType = @event.GetType();
            if (_transitionRoutes.ContainsKey(eventType))
            {
                _state = _transitionRoutes[eventType](state, @event);
            }
            Version++;
        }

        public void Hydrate(object @event)
        {
            ApplyEvent(_state, @event);
        }
    }
}
