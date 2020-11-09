using CQRS.Essentials.Abstractions.CQRS;
using Infrastructure.EventStore.Abstractions;
using Infrastructure.Storage.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.Essentials.CQRS
{
    public class Builder : IBuilder
    {
        private readonly IDictionary<string, DenormalizerDesc> _denormalizerDescriptors = new Dictionary<string, DenormalizerDesc>();
        private readonly IDictionary<string, List<KeyValuePair<string, Func<object, Task>>>> _eventHandlers = new Dictionary<string, List<KeyValuePair<string, Func<object, Task>>>>();
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly AutoResetEvent _processingLock = new AutoResetEvent(true);

        public Builder(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public void RegisterDenormalizer(DenormalizerDesc descriptor)
        {
            _denormalizerDescriptors[descriptor.Model.FullName] = descriptor;
        }

        public void RegisterEventHandler<TModel, TEvent>(Func<IDenormalizerContext<TModel>, TEvent, Task> handler) where TModel : class
        {
            var typeName = typeof(TEvent).Name;
            if (!_eventHandlers.TryGetValue(typeName, out var handlers)) //if no event hendlers ever registered then set default list of event handlers
            {
                handlers = new List<KeyValuePair<string, Func<object, Task>>>();
                _eventHandlers[typeName] = handlers;
            }
            //add event handler
            handlers.Add(new KeyValuePair<string, Func<object, Task>>(
                typeof(TModel).FullName,
                async (ev) =>
                {
                    var ctx = BuildContext<TModel>(); 
                    await handler(ctx, (TEvent) ev);
                }));
        }

        /// <summary>
        /// Read model Denormalizer Event Handlers
        /// </summary>
        /// <param name="eventData"></param>
        /// <returns>Task</returns>
        public async Task Handle(IEventData eventData)
        {
            if (!_eventHandlers.TryGetValue(eventData.EventType, out var eventHandlers)) return;

            _processingLock.WaitOne();
            try
            {
                var tasks = new List<Task>();
                foreach (var eventHandler in eventHandlers)
                {
                    tasks.Add(RunEventHandler(eventHandler, eventData));
                }

                await Task.WhenAll(tasks);
            }
            finally
            {
                _processingLock.Set();
            }
        }

        private async Task RunEventHandler(
            KeyValuePair<string, Func<object, Task>> eventHandler,
            IEventData eventData
        )
        {
            var eventHandlerFunc = eventHandler.Value;
            var readModelName = eventHandler.Key;
            try
            {
               await eventHandlerFunc(eventData.Event);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Read model {0} handler for {1} failed with event at {2}@{3}: {4}", 
                    readModelName, eventData.EventType, eventData.EventNumber, eventData.StreamId, ex);
                //you can pass in logger and log exception as well
            }
        }

        private IDenormalizerContext<TModel> BuildContext<TModel>() where TModel : class
        {
            var repository = _repositoryFactory.Create<TModel>();
            var lookups = new Dictionary<string, object>();
            foreach (var lookupType in _denormalizerDescriptors[typeof(TModel).FullName].Lookups)
            {
                lookups[lookupType.FullName] = _repositoryFactory.Create(lookupType);
            }
            return new DenormalizerContext<TModel>(repository, lookups);
        }
    }
}