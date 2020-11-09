using Inventory.Domain.Aggregates;
using Inventory.Domain.Models.Commands;
using Inventory.Api.Handlers.Query;
using Inventory.Api.Queries;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Infrastructure.Storage.Abstractions;
using Infrastructure.Storage.AzureTableStorage;
using Infrastructure.DependencyInjection.Microsoft;
using Inventory.Domain.ReadModels.Rooms;
using Inventory.Api.Handlers.Command;
using EventStore.ClientAPI;
using Infrastructure.EventStore;
using CQRS.Essentials.Abstractions.CQRS;
using CQRS.Essentials.Abstractions.DDD;
using CQRS.Essentials.Abstractions.ES;
using CQRS.Essentials.ES;
using CQRS.Essentials.CQRS;
using CQRS.Essentials.DDD;
using Infrastructure.EventStore.Abstractions;
using Infrastructure.Publishers.AzureServiceBus;
using Infrastructure.Publishers.Abstractions;

[assembly: FunctionsStartup(typeof(Inventory.Api.Startup))]
namespace Inventory.Api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder hostBuilder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();

            //set up event store
            hostBuilder.Services.AddSingleton<IEventStoreConnection>(s => new EventStoreConnectionFactory(configuration).Create(s));
            hostBuilder.Services.AddSingleton<IEventStore, Infrastructure.EventStore.EventStore>(s =>
            {
                var con = s.GetRequiredService<IEventStoreConnection>();
                var eventstore = new Infrastructure.EventStore.EventStore(con);
                return eventstore;
            });

            var configRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            hostBuilder.Services.AddSingleton(sp => configRoot);

            //set up external service bus event publisher
            var externalBusEventPublisher = hostBuilder.Services.BuildServiceProvider().GetService<IPublisher>();
            if (externalBusEventPublisher == null)
            {
                string serviceBusConnectionString = configRoot["HOTEL_MANAGEMENT_SERVICEBUS"];
                string topicName = configRoot["INVENTORY_EVENTS_TOPIC_NAME"];
                externalBusEventPublisher = new ServiceBusPublisher(serviceBusConnectionString, topicName);
                hostBuilder.Services.AddSingleton(externalBusEventPublisher);
            }
            //set up repositories
            RegisterRepositories(hostBuilder, configRoot);
            ////repo factory set up
            var repositoryFactory = new RepositoryFactory(new DependencyResolver(hostBuilder.Services));
            hostBuilder.Services.AddScoped<IRepositoryFactory>(x =>
            {
                return repositoryFactory;
            });
            //bus wiring
            var bus = new Bus();
            var builder = new Builder(repositoryFactory);
            //register read model denormalizers
            RegisterReadModelDenormalizers(builder);
            //register builder handler with bus
            bus.RegisterEventHandler(builder.Handle);
            //register event store wrapper client
            var serviceProvider = hostBuilder.Services.BuildServiceProvider();
            var eventStore = serviceProvider.GetRequiredService<IEventStore>();
            var eventStoreClient = new EventStoreClient(bus, eventStore, externalBusEventPublisher);
            hostBuilder.Services.AddSingleton<IEventStoreClient>(sp => { return eventStoreClient; });
            //set up aggregate factories di
            RegisterAggregateFactories(hostBuilder);
            //set up command handlers
            RegisterCommandHandlers(hostBuilder, bus, eventStoreClient);
            //set up query handlers
            RegisterQueryHandlers(hostBuilder, bus);
            //register bus
            hostBuilder.Services.AddSingleton<IBus>(sp => { return bus; });
        }

        private void RegisterAggregateFactories(IFunctionsHostBuilder hostBuilder)
        {
            //aggregate factories registration
            hostBuilder.Services.AddTransient<IAggregateFactory<Room>, AggregateFactory<Room>>();
        }

        private void RegisterCommandHandlers(IFunctionsHostBuilder hostBuilder, IBus bus, IEventStoreClient eventStoreClient)
        {
            var serviceProvider = hostBuilder.Services.BuildServiceProvider();
            //aggregate factories
            var roomFactory = serviceProvider.GetRequiredService<IAggregateFactory<Room>>();
            //command handler registrations
            bus.RegisterCommandHandler<AddRoom>(new AddRoomCommandHandler(roomFactory, eventStoreClient).Handle);
        }

        private void RegisterQueryHandlers(IFunctionsHostBuilder hostBuilder, IBus bus)
        {
            var serviceProvider = hostBuilder.Services.BuildServiceProvider();
            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            //query handler registrations
            bus.RegisterQueryHandler<FindRoomQuery, RoomsReadModel>(new FindRoomQueryHandler(repositoryFactory).Handle);
        }

        private void RegisterReadModelDenormalizers(IBuilder builder)
        {
            //read model denormalizers
            new RoomsDenormalizer(builder);
        }

        private void RegisterRepositories(IFunctionsHostBuilder hostBuilder, IConfigurationRoot configRoot)
        {
            hostBuilder.Services.AddScoped<IRepository<RoomsReadModel>>(x =>
            {
                var inventoryRoomsCloudTable = GetTable(configRoot["InventoryReadModels-TableStorage-ConnectionString"], configRoot["InventoryRoomsReadModel-TableName"]);
                //set up cloudtables for each read model
                return new Repository<RoomsReadModel>(inventoryRoomsCloudTable);
            });

            //repo factory set up
            hostBuilder.Services.AddScoped<IRepositoryFactory>(x =>
            {
                //set up cloudtables for each read model
                return new RepositoryFactory(new DependencyResolver(hostBuilder.Services));
            });
        }

        private CloudTable GetTable(string connection, string tableName)
        {
            CloudTableClient tableClient = CloudStorageAccount.Parse(connection).CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            return table;
        }
    }
}
