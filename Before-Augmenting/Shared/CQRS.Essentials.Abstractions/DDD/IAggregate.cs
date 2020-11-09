using System.Collections.Generic;

namespace CQRS.Essentials.Abstractions.DDD
{
    public interface IAggregate
    {
        long Version { get; }
        void Hydrate(object @event);
        IEnumerable<object> UnCommitedEvents { get; }
        void ClearUnCommitedEvents();
    }
}
