using Infrastructure.EventStore.Abstractions;
using System;
using System.Threading.Tasks;

namespace CQRS.Essentials.Abstractions.CQRS
{
    public interface IBuilder
    {
        void RegisterDenormalizer(DenormalizerDesc descriptor);
        void RegisterEventHandler<TModel, TEvent>(Func<IDenormalizerContext<TModel>, TEvent, Task> eventHandler) where TModel : class;
        Task Handle(IEventData eventData);
    }
}