using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Essentials.Abstractions.CQRS;
using Infrastructure.EventStore.Abstractions;

namespace CQRS.Essentials.CQRS
{
    public class Bus : IBus
    {
        private readonly IDictionary<string, Func<object, CancellationToken, Task<object[]>>> _commandHandlers = new Dictionary<string, Func<object, CancellationToken, Task<object[]>>>();
        private readonly IList<Func<IEventData, Task>> _eventHandlers = new List<Func<IEventData, Task>>();
        private readonly IDictionary<string, Func<object, CancellationToken, Task<object>>> _queryHandlers = new Dictionary<string, Func<object, CancellationToken, Task<object>>>();

        public async Task<object[]> Send<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : ICommand
        {
            //if no handler found you can throw exception, you can also do retry attempts here if any failures as well 
            return await _commandHandlers[typeof(TCommand).Name](command, cancellationToken);
        }

        public async Task<TResult> Send<TQuery, TResult>(TQuery query, CancellationToken cancellationToken) where TQuery : IQuery<TResult>
        {

            //if no handler found you can throw exception, you can also do retry attempts here if any failures as well 
            return (TResult)await _queryHandlers[typeof(TQuery).Name](query, cancellationToken);
        }

        public void RegisterCommandHandler<TCommand>(Func<TCommand, CancellationToken, Task<object[]>> commandHandler) where TCommand : ICommand
        {
            this._commandHandlers.Add(typeof(TCommand).Name, async (cmd, cancellationToken) => await commandHandler((TCommand)cmd, cancellationToken));
        }

        public void RegisterEventHandler(Func<IEventData, Task> handler)
        {
            _eventHandlers.Add(handler);
        }

        public void RegisterQueryHandler<TQuery, TResult>(Func<TQuery, CancellationToken, Task<TResult>> queryHandler) where TQuery : IQuery<TResult>
        {
            this._queryHandlers.Add(typeof(TQuery).Name, async (query, cancellationToken) => await queryHandler((TQuery)query, cancellationToken));
        }

        public async Task Publish(IEventData eventData)
        {
            //Publish must never throw
            try
            {
                var tasks = new List<Task>();
                foreach (var handler in _eventHandlers)
                {
                    tasks.Add(handler(eventData));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                //log it if you need to!!
                //"Publish failed"
            }
        }
    }
}