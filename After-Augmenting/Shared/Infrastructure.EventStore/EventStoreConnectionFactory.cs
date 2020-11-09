using System;
using System.Configuration;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.EventStore
{
    public class EventStoreConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public EventStoreConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEventStoreConnection Create(IServiceProvider serviceProvider)
        {
            // for backward compatibility we keep the EventStoreEndpoint settings
            var eventStoreEndpoint = _configuration[ConfigKeys.EventStoreEndpoint];
            var eventStoreConnectionString = _configuration[ConfigKeys.EventStoreConnectionString];
            var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID", EnvironmentVariableTarget.Process);
            var connectionName = $"{GetType().Namespace}:{instanceId ?? "NO_ID"}";
            IEventStoreConnection conn;
            if (!string.IsNullOrWhiteSpace(eventStoreConnectionString))
            {
                conn = EventStoreConnection.Create(eventStoreConnectionString, connectionName);
            }
            else if (!string.IsNullOrWhiteSpace(eventStoreEndpoint))
            {
                conn = EventStoreConnection.Create(ConnectionSettings.Default, new Uri(eventStoreEndpoint), connectionName);
            }
            else
            {
                throw new ConfigurationErrorsException($"Either {ConfigKeys.EventStoreEndpoint} or {ConfigKeys.EventStoreConnectionString} must be set");
            }

            // A failure of connect means the endpoint cannot be resolved (either dns or cluster discovery)
            conn.ConnectAsync().Wait();
            // Passed this point doesn't mean we are connected, but the connection will enqueue the operations
            // if the connection ends up not connecting the enqueued operations/subscriptions will fail with a ConnectionClosedException  
            return conn;
        }
    }
}