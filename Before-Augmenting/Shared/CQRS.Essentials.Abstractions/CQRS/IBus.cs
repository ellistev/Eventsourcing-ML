using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.EventStore.Abstractions;
using Infrastructure.Publishers.Abstractions;

namespace CQRS.Essentials.Abstractions.CQRS
{
    public interface IBus : IPublisher
    {
        Task<object[]> Send<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand;
        Task<TResult> Send<TQuery, TResult>(TQuery query, CancellationToken cancellationToken) where TQuery : IQuery<TResult>;
        void RegisterCommandHandler<TCommand>(Func<TCommand, CancellationToken, Task<object[]>> commandHandler) where TCommand : ICommand;
        void RegisterEventHandler(Func<IEventData, Task> handler);
        void RegisterQueryHandler<TQuery, TResult>(Func<TQuery, CancellationToken, Task<TResult>> queryHandler) where TQuery : IQuery<TResult>;
    }
}