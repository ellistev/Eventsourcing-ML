using Reservations.Api.Handlers.Query;
using Reservations.Api.Queries;
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
using Reservations.Api.Handlers.Command;
using Reservations.Domain.Models.Commands;
using Reservations.Domain.Aggregates;
using CQRS.Essentials.Abstractions.DDD;
using CQRS.Essentials.Abstractions.CQRS;
using CQRS.Essentials.Abstractions.ES;
using CQRS.Essentials.ES;
using CQRS.Essentials.CQRS;
using CQRS.Essentials.DDD;
using Infrastructure.EventStore.Abstractions;
using Infrastructure.Publishers.AzureServiceBus;
using Infrastructure.Publishers.Abstractions;
using Reservations.Domain.ReadModels.Hotel;
using Reservations.Domain.ReadModels.Location;
using Reservations.Domain.ReadModels.LocationWeather;
using Reservations.Domain.ReadModels.Room;
using Reservations.Domain.ReadModels.RoomTemperatureViews;
using Reservations.Domain.ReadModels.RoomViews;
using Reservations.Domain.ReadModels.User;

[assembly: FunctionsStartup(typeof(Reservations.Api.Startup))]
namespace Reservations.Api
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
                string topicName = configRoot["RESERVATIONS_EVENTS_TOPIC_NAME"];
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
            hostBuilder.Services.AddTransient<IAggregateFactory<Reservation>, AggregateFactory<Reservation>>();
        }

        private void RegisterCommandHandlers(IFunctionsHostBuilder hostBuilder, IBus bus, IEventStoreClient eventStoreClient)
        {
            var serviceProvider = hostBuilder.Services.BuildServiceProvider();
            //aggregate factories
            var reservationFactory = serviceProvider.GetRequiredService<IAggregateFactory<Reservation>>();
            //command handler registrations
            bus.RegisterCommandHandler<MakeReservation>(new MakeReservationCommandHandler(reservationFactory, eventStoreClient).Handle);
            bus.RegisterCommandHandler<ViewRoom>(new ViewRoomCommandHandler(reservationFactory, eventStoreClient).Handle);
        }

        private void RegisterQueryHandlers(IFunctionsHostBuilder hostBuilder, IBus bus)
        {
            var serviceProvider = hostBuilder.Services.BuildServiceProvider();
            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            //query handler registrations
            bus.RegisterQueryHandler<FindReservationQuery, ReservationsReadModel>(new FindReservationQueryHandler(repositoryFactory).Handle);
        }

        private void RegisterRepositories(IFunctionsHostBuilder hostBuilder, IConfigurationRoot configRoot)
        {
            hostBuilder.Services.AddScoped<IRepository<ReservationsReadModel>>(x =>
            {
                var reservationCloudTable = GetTable(configRoot["ReservationsReadModels-TableStorage-ConnectionString"], configRoot["ReservationsReadModel-TableName"]);
                //set up cloudtables for each read model
                return new Repository<ReservationsReadModel>(reservationCloudTable);
            });

            hostBuilder.Services.AddScoped<IRepository<RoomTypeAvailabilityReadModel>>(x =>
            {
                var roomTypeAvailabilityCloudTable = GetTable(configRoot["ReservationsReadModels-TableStorage-ConnectionString"], configRoot["RoomTypeAvailabilityReadModel-TableName"]);
                //set up cloudtables for each read model
                return new Repository<RoomTypeAvailabilityReadModel>(roomTypeAvailabilityCloudTable);
            });
            
            hostBuilder.Services.AddScoped<IRepository<RoomsReadModel>>(x =>
            {
                var roomTypeAvailabilityCloudTable = GetTable(configRoot["ReservationsReadModels-TableStorage-ConnectionString"], configRoot["RoomsReadModel-TableName"]);
                //set up cloudtables for each read model
                return new Repository<RoomsReadModel>(roomTypeAvailabilityCloudTable);
            });            
            
            hostBuilder.Services.AddScoped<IRepository<UsersReadModel>>(x =>
            {
                var roomTypeAvailabilityCloudTable = GetTable(configRoot["ReservationsReadModels-TableStorage-ConnectionString"], configRoot["UsersReadModel-TableName"]);
                //set up cloudtables for each read model
                return new Repository<UsersReadModel>(roomTypeAvailabilityCloudTable);
            });
            
            hostBuilder.Services.AddScoped<IRepository<HotelsReadModel>>(x =>
            {
                var roomTypeAvailabilityCloudTable = GetTable(configRoot["ReservationsReadModels-TableStorage-ConnectionString"], configRoot["HotelsReadModel-TableName"]);
                //set up cloudtables for each read model
                return new Repository<HotelsReadModel>(roomTypeAvailabilityCloudTable);
            });
            
            hostBuilder.Services.AddScoped<IRepository<RoomViewsReadModel>>(x =>
            {
                var roomTypeAvailabilityCloudTable = GetTable(configRoot["ReservationsReadModels-TableStorage-ConnectionString"], configRoot["RoomViewsReadModel-TableName"]);
                //set up cloudtables for each read model
                return new Repository<RoomViewsReadModel>(roomTypeAvailabilityCloudTable);
            });
        }

        private void RegisterReadModelDenormalizers(IBuilder builder)
        {
            //read model denormalizers
            new ReservationsDenormalizer(builder);
            new HotelsDenormalizer(builder);
            new RoomViewsDenormalizer(builder);
            new UsersDenormalizer(builder);
            new RoomsDenormalizer(builder);
        }

        private CloudTable GetTable(string connection, string tableName)
        {
            CloudTableClient tableClient = CloudStorageAccount.Parse(connection).CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);
            return table;
        }
    }
}
