using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using EventStore.ClientAPI;
using Infrastructure.EventStore;
using Infrastructure.Storage.Abstractions;
using Infrastructure.Storage.AzureTableStorage;
using Infrastructure.DependencyInjection.Microsoft;
using Reservations.Domain.ReadModels.Reservation;
using CQRS.Essentials.Abstractions.CQRS;
using CQRS.Essentials.Abstractions.ES;
using CQRS.Essentials.ES;
using CQRS.Essentials.CQRS;
using Infrastructure.EventStore.Abstractions;
using Infrastructure.Publishers.AzureServiceBus;
using Infrastructure.Publishers.Abstractions;

[assembly: FunctionsStartup(typeof(HotelManagement.Processor.Startup))]
namespace HotelManagement.Processor
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
            //register bus
            hostBuilder.Services.AddSingleton<IBus>(sp => { return bus; });
        }

        private void RegisterRepositories(IFunctionsHostBuilder hostBuilder, IConfigurationRoot configRoot)
        {
            hostBuilder.Services.AddScoped<IRepository<RoomTypeAvailabilityReadModel>>(x =>
            {
                var roomTypeAvailabilityCloudTable = GetTable(configRoot["ReservationsReadModels-TableStorage-ConnectionString"], configRoot["RoomTypeAvailabilityReadModel-TableName"]);
                //set up cloudtables for each read model
                return new Repository<RoomTypeAvailabilityReadModel>(roomTypeAvailabilityCloudTable);
            });
        }

        private void RegisterReadModelDenormalizers(IBuilder builder)
        {
            //read model denormalizers
            new RoomTypeAvailabilityDenormalizer(builder);
        }

        private CloudTable GetTable(string connection, string tableName)
        {
            CloudTableClient tableClient = CloudStorageAccount.Parse(connection).CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            return table;
        }
    }
}
