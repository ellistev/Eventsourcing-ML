using System;
using Infrastructure.EventStore.Abstractions;

namespace Infrastructure.EventStore
{
    public class Position : IPosition
    {
        public Position(global::EventStore.ClientAPI.Position position)
        {
            Value = position;
        }

        public global::EventStore.ClientAPI.Position Value { get; }

        public int CompareTo(IPosition other)
        {
            var right = other as Position;
            if (right == null) throw new ArgumentException("other must be a EventStorePosition");
            if (Value < right.Value) return -1;
            if (Value > right.Value) return 1;
            return 0;
        }
    }
}